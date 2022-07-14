using System;
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

        public AggieEnterpriseJournalJob(SlothDbContext context, IAggieEnterpriseService aggieEnterpriseService)
        {
            _context = context;
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        public async Task UploadTransactions(ILogger log)
        {
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
                            }
                            else if (requestStatus.RequestId.HasValue &&
                                     requestStatus.RequestStatus == RequestStatus.Rejected)
                            {
                                // failure, update transaction status to rejected
                                transaction.SetStatus(TransactionStatuses.Rejected);

                                journalRequest.RequestId = requestStatus.RequestId.Value;
                                journalRequest.Status = requestStatus.RequestStatus.ToString();
                            }

                            // TODO: These are likely the only two statuses possible for a new request, but confirm

                            // save changes
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error creating journal for transaction {TransactionId}", transaction.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error uploading transactions");
            }
        }

        /// <summary>
        /// Find all transactions that have a status of "Processing" and have an associated journal request.
        /// Query the Aggie Enterprise API to determine the status of those requests and move into appropriate status.
        /// </summary>
        public async Task ResolveProcessingJournals(ILogger log)
        {
            try
            {
                // fetch staged transactions with FinancialSegmentString populated
                var transactions = await _context.Transactions
                    .Where(t => t.Status == TransactionStatuses.Processing)
                    .Where(t => t.JournalRequest != null)
                    .Include(t=>t.JournalRequest)
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
                        try
                        {
                            var result = await _aggieEnterpriseService.GetJournalStatus(transaction.JournalRequest.RequestId.ToString());

                            if (result?.GlJournalRequestStatus == null)
                            {
                                log.Error("Error getting status of journal request for transaction {TransactionId}",
                                    transaction.Id);
                            }
                            else if (result.GlJournalRequestStatus.RequestStatus.RequestStatus ==
                                     RequestStatus.Rejected)
                            {
                                // failure, update transaction status to rejected
                                transaction.SetStatus(TransactionStatuses.Rejected);
                                // TODO: do we want to save any metadata about request?
                                transaction.JournalRequest.Status = result.GlJournalRequestStatus.RequestStatus.RequestStatus.ToString();
                            }
                            else if (result.GlJournalRequestStatus.RequestStatus.RequestStatus ==
                                     RequestStatus.Complete)
                            {
                                // success, update transaction status to uploaded
                                transaction.SetStatus(TransactionStatuses.Completed);
                                transaction.JournalRequest.Status = result.GlJournalRequestStatus.RequestStatus.RequestStatus.ToString();
                            }
                            else
                            {
                                // still processing, do nothing
                                // TODO: if not processing we might want to log, just in case we get a weird response
                            }

                            // save changes
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error checking status of journal for transaction {TransactionId}",
                                transaction.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error processing journal statuses");
            }
        }
    }
}
