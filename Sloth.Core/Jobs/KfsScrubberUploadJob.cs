using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Core.Jobs
{
    public class KfsScrubberUploadJob
    {
        private readonly SlothDbContext _context;
        private readonly IKfsScrubberService _kfsScrubberService;

        public const string JobName = "Kfs.ScrubberUpload";

        public KfsScrubberUploadJob(SlothDbContext context, IKfsScrubberService kfsScrubberService)
        {
            _context = context;
            _kfsScrubberService = kfsScrubberService;
        }

        public async Task<KfsScrubberUploadJobDetails> UploadScrubber(ILogger log)
        {
            var scrubberJobDetails = new KfsScrubberUploadJobDetails();
            try
            {
                // fetch staged transactions with accounts populated
                var transactions = await _context.Transactions
                    .Where(t => t.Transfers.Any(tran => tran.Account != null))
                    .Where(t => t.Status == TransactionStatuses.Scheduled)
                    .Include(t => t.Transfers)
                    .Include(t => t.Source)
                        .ThenInclude(s => s.Team)
                    .Include(t => t.ReversalOfTransaction)
                    .ToListAsync();

                if (!transactions.Any())
                {
                    log.Information("No scheduled transactions found.");
                    scrubberJobDetails.Message = "No scheduled transactions found.";
                    return scrubberJobDetails;
                }

                // group transactions by origin code and feed
                var groups = transactions.GroupBy(t => t.Source);
                foreach (var group in groups)
                {
                    var transactionGroupDetails = new KfsTransactionGroupDetails();
                    scrubberJobDetails.TransactionGroups.Add(transactionGroupDetails);
                    try
                    {
                        var source = group.Key;
                        var groupedTransactions = group.ToList();
                        transactionGroupDetails.TransactionCount = groupedTransactions.Count;

                        var originCode = source.OriginCode;

                        // create scrubber
                        log.Information("Creating Scrubber for {count} transactions.", groupedTransactions.Count);
                        var scrubber = new Scrubber()
                        {
                            BatchDate = DateTime.Today,
                            BatchSequenceNumber = 1,
                            Transactions = groupedTransactions,
                            Source = source
                        };

                        // create filename
                        var prefix = DocumentTypes.GetDocumentTypeFilePrefix(source.DocumentType);
                        var filename = $"{prefix}.{originCode}.{DateTime.UtcNow:yyyyMMddHHmmssffff}.xml";

                        // ship scrubber
                        log.Information("Uploading {filename}", filename);
                        var username = source.KfsFtpUsername;
                        var passwordKeyName = source.KfsFtpPasswordKeyName;
                        var blob = await _kfsScrubberService.UploadScrubber(scrubber, filename, username,
                            passwordKeyName, log);
                        scrubber.Uri = blob?.Uri ?? ""; //TODO: remove Uri from scrubber table after ensureing data is in Blob table
                        scrubber.Blob = blob;
                        scrubber.BlobId = blob?.Id;

                        // persist scrubber uri
                        _context.Scrubbers.Add(scrubber);

                        // update transactions' status and jobRecord
                        groupedTransactions.ForEach(t =>
                        {
                            t.SetStatus(TransactionStatuses.Completed);
                        });

                        // save per scrubber
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, ex.Message);
                        log.Error($"KFS Upload error for source {group.Key.Name}");
                        transactionGroupDetails.Message = $"KFS Upload error for source {group.Key.Name}";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                scrubberJobDetails.Message = "KFS Upload error";
            }

            return scrubberJobDetails;
        }

        public class KfsScrubberUploadJobDetails
        {
            public List<KfsTransactionGroupDetails> TransactionGroups { get; set; } = new();
            public string Message { get; set; }
        }

        public class KfsTransactionGroupDetails
        {
            public string ScrubberId { get; set; }
            public int TransactionCount { get; set; }
            public string Message { get; set; }
        }
    }
}
