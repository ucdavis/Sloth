using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Jobs.Kfs.ScrubberUpload.Services;

namespace Sloth.Jobs.Kfs.ScrubberUpload
{
    public class UploadScrubberJob
    {
        private readonly ILogger _log;
        private readonly SlothDbContext _context;
        private readonly IKfsScrubberService _kfsScrubberService;


        public UploadScrubberJob(ILogger log, SlothDbContext context, IKfsScrubberService kfsScrubberService)
        {
            _log = log;
            _context = context;
            _kfsScrubberService = kfsScrubberService;
        }

        public async Task UploadScrubber()
        {
            try
            {
                // fetch all staged transactions
                var transactions = _context.Transactions
                    .Where(t => t.Status == TransactionStatuses.Scheduled)
                    .Include(t => t.Transfers)
                    .Include(t => t.Source)
                    .ToList();

                if (!transactions.Any())
                {
                    _log.Information("No scheduled transactions found.");
                    return;
                }

                // group transactions by origin code and feed
                var groups = transactions.GroupBy(t => t.Source);
                foreach (var group in groups)
                {
                    var groupedTransactions = group.ToList();

                    var originCode = group.Key.OriginCode;
                    var docType = group.Key.DocumentType;

                    // create scrubber
                    _log.Information("Creating Scrubber for {count} transactions.", groupedTransactions.Count);
                    var scrubber = new Scrubber()
                    {
                        Chart               = "3",
                        OrganizationCode    = "ACCT",
                        BatchDate           = DateTime.Today,
                        BatchSequenceNumber = 1,
                        Transactions        = groupedTransactions,
                        OriginCode          = originCode,
                        DocumentType        = docType
                    };

                    // create filename
                    var filename = $"{docType}.{originCode}.{DateTime.UtcNow:yyyyMMddHHmmssffff}.xml";

                    // ship scrubber
                    _log.Information("Uploading {filename}", filename);
                    var uri = await _kfsScrubberService.UploadScrubber(scrubber, filename, _log);
                    scrubber.Uri = uri.AbsoluteUri;

                    // persist scrubber uri
                    _context.Scrubbers.Add(scrubber);

                    // update transactions' status
                    groupedTransactions.ForEach(t =>
                    {
                        t.Status = TransactionStatuses.Completed;
                    });
                }
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
