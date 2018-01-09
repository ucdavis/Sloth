using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Jobs.Services;

namespace Sloth.Jobs.Jobs
{
    public class UploadScrubberJob : JobBase
    {
        private readonly SlothDbContext _context;
        private readonly IKfsScrubberService _kfsScrubberService;


        public UploadScrubberJob(SlothDbContext context, IKfsScrubberService kfsScrubberService) : base("UploadScrubber")
        {
            _context = context;
            _kfsScrubberService = kfsScrubberService;
        }

        [RecurringJob(CronStrings.EndOfBusiness, RecurringJobId = "upload-nightly-scrubber")]
        public async Task UploadScrubber(PerformContext context)
        {
            SetupLogging(context);

            var log = Logger;

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
                    log.Information("No scheduled transactions found.");
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
                    log.Information("Creating Scrubber for {count} transactions.", groupedTransactions.Count);
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
                    log.Information("Uploading {filename}", filename);
                    var uri = await _kfsScrubberService.UploadScrubber(scrubber, filename, log);
                    scrubber.Uri = uri.AbsoluteUri;

                    // persist scrubber uri
                    _context.Scrubbers.Add(scrubber);
                }

                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
