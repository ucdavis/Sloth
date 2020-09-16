using System;
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

        public static string JobName = "Kfs.ScrubberUpload";

        public KfsScrubberUploadJob(SlothDbContext context, IKfsScrubberService kfsScrubberService)
        {
            _context = context;
            _kfsScrubberService = kfsScrubberService;
        }

        public async Task UploadScrubber(ILogger log, KfsScrubberUploadJobRecord jobRecord)
        {
            try
            {
                // fetch all staged transactions
                var transactions = await _context.Transactions
                    .Where(t => t.Status == TransactionStatuses.Scheduled)
                    .Include(t => t.Transfers)
                    .Include(t => t.Source)
                        .ThenInclude(s => s.Team)
                    .Include(t => t.ReversalOfTransaction)
                    .ToListAsync();

                if (!transactions.Any())
                {
                    log.Information("No scheduled transactions found.");
                    return;
                }

                // group transactions by origin code and feed
                var groups = transactions.GroupBy(t => t.Source);
                foreach (var group in groups)
                {
                    try
                    {
                        var source = group.Key;
                        var groupedTransactions = group.ToList();

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
                            t.Status = TransactionStatuses.Completed;
                            t.KfsScrubberUploadJobRecordId = jobRecord.Id;
                        });

                        // save per scrubber
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, ex.Message);
                        log.Error($"KFS Upload error for source {group.Key.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
            }
        }
    }
}
