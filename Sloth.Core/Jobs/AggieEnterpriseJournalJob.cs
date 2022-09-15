using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AggieEnterpriseApi;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Core.Jobs
{
    public class AggieEnterpriseJournalJob
    {
        private readonly SlothDbContext _context;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public static string JobName = "AggieEnterprise.JournalProcessor";
        public static string JobNameUploadTransactions = nameof(AggieEnterpriseJournalJob) + "." + nameof(UploadTransactions);
        public static string JobNameResolveProcessingJournals = nameof(AggieEnterpriseJournalJob) + "." + nameof(ResolveProcessingJournals);

        public AggieEnterpriseJournalJob(SlothDbContext context, IAggieEnterpriseService aggieEnterpriseService)
        {
            _context = context;
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        public class AggieEnterpriseJournalJobDetails
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
        }

        public async Task UploadTransactions(ILogger log)
        {
            var jobRun = new JobRecord
                { Name = JobNameUploadTransactions, Status = JobRecord.Statuses.Running };

            _context.JobRecords.Add(jobRun);

            await _context.SaveChangesAsync();

            var jobDetails = new AggieEnterpriseJournalJobDetails();

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
                    return;
                }

                // group transactions by origin code and feed
                var groups = transactions.GroupBy(t => t.Source);
                foreach (var group in groups)
                {
                    var source = group.Key;
                    var groupedTransactions = group.ToList();

                    log.Information("Uploading {TransactionCount} transactions for {SourceName}",
                        groupedTransactions.Count, source.Name);

                    // loop through grouped transactions and upload to Aggie Enterprise, saving status of each
                    foreach (var transaction in groupedTransactions)
                    {
                        var transactionRunStatus = new AggieEnterpriseJournalJobDetails.TransactionRunStatus
                            { TransactionId = transaction.Id };

                        try
                        {
                            var result = await _aggieEnterpriseService.CreateJournal(source, transaction);
                            var requestStatus = result.GlJournalRequest.RequestStatus;

                            // here we will store the result of the transaction upload
                            var journalRequest = new JournalRequest
                                { Transactions = new[] { transaction }, Source = source };

                            if (requestStatus.RequestId.HasValue &&
                                requestStatus.RequestStatus == RequestStatus.Pending)
                            {
                                // success, update transaction status to uploaded
                                transaction.SetStatus(TransactionStatuses.Processing);

                                journalRequest.RequestId = requestStatus.RequestId.Value;
                                journalRequest.Status = requestStatus.RequestStatus.ToString();

                                // save journal request
                                _context.JournalRequests.Add(journalRequest);

                                transactionRunStatus.Action = requestStatus.RequestStatus.ToString();
                                ;
                            }
                            else if (requestStatus.RequestId.HasValue &&
                                     requestStatus.RequestStatus == RequestStatus.Rejected)
                            {
                                // TODO: show rejection reason in SetStatus

                                // failure, update transaction status to rejected
                                transaction.SetStatus(TransactionStatuses.Rejected);

                                journalRequest.RequestId = requestStatus.RequestId.Value;
                                journalRequest.Status = requestStatus.RequestStatus.ToString();

                                transactionRunStatus.Action = requestStatus.RequestStatus.ToString();
                            }

                            // TODO: These are likely the only two statuses possible for a new request, but confirm

                            // save changes
                            await _context.SaveChangesAsync();
                        }
                        catch (ArgumentException ex)
                        {
                            log.Error(ex, "Error creating journal for transaction {TransactionId}", transaction.Id);
                            transactionRunStatus.Action = "Error";

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
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error uploading transactions");
            }

            jobRun.SetCompleted(JobRecord.Statuses.Success, jobDetails);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Find all transactions that have a status of "Processing" and have an associated journal request.
        /// Query the Aggie Enterprise API to determine the status of those requests and move into appropriate status.
        /// </summary>
        public async Task ResolveProcessingJournals(ILogger log)
        {
            var jobRun = new JobRecord
                { Name = JobNameResolveProcessingJournals, Status = JobRecord.Statuses.Running };

            _context.JobRecords.Add(jobRun);

            await _context.SaveChangesAsync();

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
                    return;
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
                                await _aggieEnterpriseService.GetJournalStatus(transaction.JournalRequest.RequestId
                                    .ToString());

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
                            }
                            else if (result.GlJournalRequestStatus.RequestStatus.RequestStatus ==
                                     RequestStatus.Complete)
                            {
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

            jobRun.SetCompleted(JobRecord.Statuses.Success, jobDetails);

            await _context.SaveChangesAsync();
        }
    }
}
