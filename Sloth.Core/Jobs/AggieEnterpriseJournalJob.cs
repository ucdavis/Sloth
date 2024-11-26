using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AggieEnterpriseApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Sloth.Core.Abstractions;
using Sloth.Core.Configuration;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Core.Jobs
{
    public class AggieEnterpriseJournalJob
    {
        private readonly SlothDbContext _context;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;
        private readonly INotificationService _notificationService;
        private readonly AggieEnterpriseOptions _options;

        public const string JobName = "AggieEnterprise.JournalProcessor";
        public const string JobNameUploadTransactions = nameof(AggieEnterpriseJournalJob) + "." + nameof(UploadTransactions);
        public const string JobNameResolveProcessingJournals = nameof(AggieEnterpriseJournalJob) + "." + nameof(ResolveProcessingJournals);

        public AggieEnterpriseJournalJob(SlothDbContext context, IAggieEnterpriseService aggieEnterpriseService, INotificationService notificationService, IOptions<AggieEnterpriseOptions> options)
        {
            _context = context;
            _aggieEnterpriseService = aggieEnterpriseService;
            _notificationService = notificationService;
            _options = options.Value;
        }

        public class AggieEnterpriseJournalJobDetails : IHasTransactionIds
        {
            public AggieEnterpriseJournalJobDetails()
            {
                TransactionRunStatuses = new List<TransactionRunStatus>();
            }

            public List<TransactionRunStatus> TransactionRunStatuses { get; set; }

            public int TransactionsProcessedCount => TransactionRunStatuses.Count;

            public class TransactionRunStatus
            {
                public string TransactionId { get; set; }
                public string Action { get; set; }
                public string Message { get; set; }
            }

            public IEnumerable<string> GetTransactionIds()
            {
                return TransactionRunStatuses?.Select(x => x.TransactionId) ?? Enumerable.Empty<string>();
            }
        }

        public async Task<AggieEnterpriseJournalJobDetails> UploadTransactions(ILogger log)
        {
            var jobDetails = new AggieEnterpriseJournalJobDetails();

            if(_options.DisableJournalUpload)
            {
                log.Warning("Journal upload is disabled in configuration");
                return jobDetails;
            }

            try
            {
                // fetch staged transactions with FinancialSegmentString populated
                var transactions = await _context.Transactions
                    .Where(t => t.Transfers.Any(tran => tran.FinancialSegmentString != null))
                    .Where(t => t.Status == TransactionStatuses.Scheduled)
                    .Include(t => t.Transfers)
                    .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                    .Include(t => t.ReversalOfTransaction)
                    .ToListAsync();

                if (!transactions.Any())
                {
                    log.Information("No scheduled transactions found");
                    return jobDetails;
                }

                // group transactions by origin code and feed
                var groups = transactions.GroupBy(t => t.Source);
                foreach (var group in groups)
                {
                    var source = group.Key;
                    var groupedTransactions = group.ToList();

                    bool hasRejectedTransactions = false;

                    log.Information("Uploading {TransactionCount} transactions for {SourceName}",
                        groupedTransactions.Count, source.Name);

                    // loop through grouped transactions and upload to Aggie Enterprise, saving status of each
                    foreach (var transaction in groupedTransactions)
                    {
                        var transactionRunStatus = new AggieEnterpriseJournalJobDetails.TransactionRunStatus
                        { TransactionId = transaction.Id };

                        try
                        {
                            // create a new trackingId for the AE Service to use
                            transaction.ConsumerTrackingId = Guid.NewGuid().ToString();

                            var result = await _aggieEnterpriseService.CreateJournal(source, transaction);
                            var requestStatus = result.GlJournalRequest.RequestStatus;

                            // here we will store the result of the transaction upload
                            var journalRequest = new JournalRequest
                            { Transactions = new[] { transaction }, Source = source };

                            if (requestStatus.RequestId.HasValue &&
                                requestStatus.RequestStatus == RequestStatus.Pending)
                            {
                                // success, update transaction status to uploaded
                                transaction.SetStatus(TransactionStatuses.Processing, details: $"RequestId: {requestStatus.RequestId}, ConsumerTrackingId: {transaction.ConsumerTrackingId}");

                                journalRequest.RequestId = requestStatus.RequestId.Value;
                                journalRequest.Status = requestStatus.RequestStatus.ToString();

                                // save journal request
                                _context.JournalRequests.Add(journalRequest);
                                transaction.JournalRequest = journalRequest;

                                transactionRunStatus.Action = requestStatus.RequestStatus.ToString();

                            }
                            else if (requestStatus.RequestId.HasValue &&
                                     requestStatus.RequestStatus == RequestStatus.Rejected)
                            {
                                // TODO: show rejection reason in SetStatus

                                hasRejectedTransactions = true;

                                // failure, update transaction status to rejected
                                transaction.SetStatus(TransactionStatuses.Rejected, details: $"RequestId: {requestStatus.RequestId}, ConsumerTrackingId: {transaction.ConsumerTrackingId}");

                                journalRequest.RequestId = requestStatus.RequestId.Value;
                                journalRequest.Status = requestStatus.RequestStatus.ToString();


                                if (result.GlJournalRequest.ValidationResults != null && result.GlJournalRequest.ValidationResults.ErrorMessages != null)
                                {
                                    var innerLog = log.ForContext("journalRequestId", journalRequest.RequestId);
                                    innerLog.Warning("journalResult {journalResult}", JsonConvert.SerializeObject(result.GlJournalRequest));
                                    foreach (var err in result.GlJournalRequest.ValidationResults.ErrorMessages)
                                    {
                                        innerLog.Warning("Transaction {TransactionId} rejected: {Message}",
                                            transaction.Id, err);
                                    }
                                }


                                transactionRunStatus.Action = requestStatus.RequestStatus.ToString();
                            }
                            else if(requestStatus.RequestStatus == RequestStatus.Validated)
                            {
                                log.Error("Unexpected Aggie Enterprise status of Validated returned.");
                            }

                            // TODO: These are likely the only two statuses possible for a new request, but confirm

                            // save changes
                            await _context.SaveChangesAsync();
                        }
                        catch (ArgumentException ex)
                        {
                            log.Error(ex, "Error creating journal for transaction {TransactionId}", transaction.Id);
                            transactionRunStatus.Action = "Error";

                            hasRejectedTransactions = true;

                            // failure because of txn args, do not retry
                            transaction.SetStatus(TransactionStatuses.Rejected, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error creating journal for transaction {TransactionId}", transaction.Id);
                            transactionRunStatus.Action = "Error";
                        }

                        jobDetails.TransactionRunStatuses.Add(transactionRunStatus);
                    }

                    if (hasRejectedTransactions)
                    {
                        var notifySuccess = await _notificationService.Notify(Notification
                            .Message("One or more transactions were rejected by Aggie Enterprise")
                            .WithEmailsToTeam(source.Team.Slug, TeamRole.Admin)
                            .WithCcEmailsToTeam(source.Team.Slug, TeamRole.Manager)
                            .WithLinkBack("View Failed Transactions", $"/{source.Team.Slug}/Reports/FailedTransactions"));
                        if (!notifySuccess)
                        {
                            log.Error("Error sending rejected transactions notification for team {TeamSlug}", source.Team.Slug);
                            //TODO: queue notification for retry
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error uploading transactions");
            }

            await _context.SaveChangesAsync();

            return jobDetails;
        }

        /// <summary>
        /// Find all transactions that have a status of "Processing" and have an associated journal request.
        /// Query the Aggie Enterprise API to determine the status of those requests and move into appropriate status.
        /// </summary>
        public async Task<AggieEnterpriseJournalJobDetails> ResolveProcessingJournals(ILogger log)
        {
            var jobDetails = new AggieEnterpriseJournalJobDetails();

            try
            {
                // fetch staged transactions with FinancialSegmentString populated
                var transactions = await _context.Transactions
                    .Where(t => t.Status == TransactionStatuses.Processing)
                    .Where(t => t.JournalRequest != null)
                    .Include(t => t.JournalRequest)
                    .Include(t => t.Source.Team)
                    .ToListAsync();

                if (!transactions.Any())
                {
                    log.Information("No scheduled transactions found");
                    return jobDetails;
                }

                var groups = transactions.GroupBy(t => t.Source.Team);
                foreach (var group in groups)
                {
                    var team = group.Key;
                    var groupedTransactions = group.ToList();

                    log.Information("Checking status of {TransactionCount} transactions for team {TeamName}",
                        groupedTransactions.Count, team.Name);

                    // loop through grouped transactions and determine status of each
                    foreach (var transaction in groupedTransactions)
                    {
                        var transactionRunStatus = new AggieEnterpriseJournalJobDetails.TransactionRunStatus
                        { TransactionId = transaction.Id };

                        try
                        {
                            var result =
                                await _aggieEnterpriseService.GetJournalStatus(transaction.JournalRequest.RequestId);

                            if (result?.GlJournalRequestStatus == null)
                            {
                                log.Error("Error getting status of journal request for transaction {TransactionId}",
                                    transaction.Id);

                                transactionRunStatus.Action = "Error";
                            }
                            else if (result.GlJournalRequestStatus.RequestStatus.RequestStatus ==
                                     RequestStatus.Rejected ||
                                     result.GlJournalRequestStatus.RequestStatus.RequestStatus == RequestStatus.Error)
                            {
                                transaction.SetStatus(TransactionStatuses.Rejected);
                                // TODO: do we want to save any metadata about request? if not, we'll need to pull status from API
                                transaction.JournalRequest.Status =
                                    result.GlJournalRequestStatus.RequestStatus.RequestStatus.ToString();

                                transactionRunStatus.Action = TransactionStatuses.Rejected;

                                try
                                {
                                    if(result.GlJournalRequestStatus.ProcessingResult != null)
                                    {
                                        var innerLog = log.ForContext("TransactionId", transaction.Id);
                                        innerLog.Information("Processing Result Status {status}", result.GlJournalRequestStatus.ProcessingResult.Status);
                                        foreach (var job in result.GlJournalRequestStatus.ProcessingResult.Jobs)
                                        {
                                            innerLog.Information("Job {jobId} Status {status} Job Report {jobReport}", job.JobId, job.JobStatus, job.JobReport);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Exception trying to process journal ProcessingResult.Jobs error", ex);
                                }
                                try
                                {
                                    if (result.GlJournalRequestStatus.RequestStatus.ErrorMessages != null)
                                    {
                                        var innerLog = log.ForContext("TransactionId", transaction.Id);
                                        innerLog.Information("RequestStatus Status {status}", result.GlJournalRequestStatus.RequestStatus.RequestStatus);
                                        foreach (var err in result.GlJournalRequestStatus.RequestStatus.ErrorMessages)
                                        {
                                            innerLog.Information("Error Message: {err}", err);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Exception trying to process journal ErrorMessages", ex);
                                }
                            }
                            else if (result.GlJournalRequestStatus.RequestStatus.RequestStatus ==
                                     RequestStatus.Complete || result.GlJournalRequestStatus.RequestStatus.RequestStatus == RequestStatus.Warning)
                            {
                                if(result.GlJournalRequestStatus.RequestStatus.RequestStatus == RequestStatus.Warning)
                                {
                                    log.Warning("Journal request for transaction {TransactionId} completed with warning", transaction.Id);
                                    transaction.SetStatus(TransactionStatuses.Completed, details: "Journal request completed with warning");
                                }
                                // success, update transaction status to uploaded
                                transaction.SetStatus(TransactionStatuses.Completed);
                                transaction.JournalRequest.Status =
                                    result.GlJournalRequestStatus.RequestStatus.RequestStatus.ToString();

                                transactionRunStatus.Action = TransactionStatuses.Completed;
                            }
                            else
                            {
                                // still processing, do nothing
                                // TODO: if not processing we might want to log, just in case we get a weird response
                                transactionRunStatus.Action = TransactionStatuses.Processing;
                            }

                            // save changes
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error checking status of journal for transaction {TransactionId}",
                                transaction.Id);

                            transactionRunStatus.Action = "Error";
                        }

                        jobDetails.TransactionRunStatuses.Add(transactionRunStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error processing journal statuses");
            }

            await _context.SaveChangesAsync();

            return jobDetails;
        }
    }
}
