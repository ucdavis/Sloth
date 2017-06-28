using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Sloth.Api.Services;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Api.Jobs
{
    public class UploadScrubberJob : JobBase
    {
        private readonly SlothDbContext _context;
        private readonly IKfsScrubberService _kfsScrubberService;


        public UploadScrubberJob(SlothDbContext context, IKfsScrubberService kfsScrubberService)
        {
            _context = context;
            _kfsScrubberService = kfsScrubberService;
        }

        [RecurringJob(CronStrings.EndOfBusiness, RecurringJobId = "upload-nightly-scrubber")]
        public async Task UploadScrubber(PerformContext context)
        {
            SetupLogging(context);

            var log = Logger.ForContext("jobId", context.BackgroundJob.Id);

            try
            {
                // fetch all staged transactions
                var transactions = _context.Transactions
                    .Where(t => t.Status == TransactionStatus.Scheduled)
                    .Include(t => t.Transfers)
                    .ToList();

                // create scrubber
                log.Information("Creating Scrubber for {count} transactions.", transactions.Count);
                var scrubber = new Scrubber()
                {
                    Chart               = "3",
                    OrganizationCode    = "ACCT",
                    BatchDate           = DateTime.Today,
                    BatchSequenceNumber = 1,
                    Transactions        = transactions
                };
                
                // create filename
                var oc = "SL";
                var filename = $"journal.{oc}.{DateTime.UtcNow:yyyyMMddHHmmssffff}.xml";

                // ship scrubber
                log.Information("Uploading {filename}", filename);
                var uri = await _kfsScrubberService.UploadScrubber(scrubber, filename, log);
                scrubber.Uri = uri.AbsoluteUri;

                // persist scrubber uri
                _context.Scrubbers.Add(scrubber);
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
