using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sloth.Core;
using Sloth.Core.Jobs;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Logging;
using Sloth.Web.Models.JobViewModels;
using Sloth.Web.Services;

namespace Sloth.Web.Controllers
{
    [Authorize(Roles = Roles.SystemAdmin)]
    public class JobsController : Controller
    {
        private readonly SlothDbContext _dbContext;
        private readonly IBackgroundTaskQueue _queue;
        private readonly ICyberSourceBankReconcileService _cyberSourceBankReconcileService;

        public JobsController(SlothDbContext dbContext, IBackgroundTaskQueue queue, ICyberSourceBankReconcileService cyberSourceBankReconcileService)
        {
            _dbContext = dbContext;
            _queue = queue;
            _cyberSourceBankReconcileService = cyberSourceBankReconcileService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> KfsScrubberUpload()
        {
            var records = await _dbContext.KfsScrubberUploadJobRecords
                .ToListAsync();

            return View(records);
        }

        public async Task<IActionResult> KfsScrubberUploadDetails(string id)
        {
            var record = await _dbContext.KfsScrubberUploadJobRecords
                .Include(r => r.Logs)
                .FirstOrDefaultAsync(r => r.Id == id);

            return View(record);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public async Task<IActionResult> KfsScrubberUploadRecords(double from, double to, int utc_offset_from, int utc_offset_to)
        {
            // js offsets are in minutes
            var startOffset = new TimeSpan(0, utc_offset_from, 0);
            var endOffset = new TimeSpan(0, utc_offset_to, 0);

            // js ticks are in milliseconds, remove offset to utc
            var start = new DateTime(1970, 1, 1).AddMilliseconds(from) - startOffset;
            var end = new DateTime(1970, 1, 1).AddMilliseconds(to) - endOffset;

            // fetch records
            var records = await _dbContext.KfsScrubberUploadJobRecords
                .Where(r => r.RanOn >= start && r.RanOn <= end)
                .ToListAsync();

            // js epoch is 1/1/1970
            var jsEpoch = new DateTime(1970, 1, 1).Ticks / 10_000;

            // format for js, add offset to local, reset to js epoch
            var events = records
                .OrderBy(r => r.RanOn)
                .Select(r => new
                {
                    id     = r.Id,
                    title  = $"{r.Name} - {r.RanOn:G}",
                    @class = "event-success",
                    url    = Url.Action(nameof(KfsScrubberUploadDetails), new { id = r.Id }),
                    start  = (r.RanOn.Ticks / 10_000) + (startOffset.Ticks / 10_000) - jsEpoch,
                    end    = (r.RanOn.Ticks / 10_000) + (startOffset.Ticks / 10_000) - jsEpoch + 1,
                });


            return new JsonResult(new
            {
                success = 1,
                result = events,
            });
        }

        [HttpPost]
        public async Task<IActionResult> RunKfsScrubberUpload()
        {
            // log run
            var record = new KfsScrubberUploadJobRecord()
            {
                Id = Guid.NewGuid().ToString(),
                Name = KfsScrubberUploadJob.JobName,
                RanOn = DateTime.UtcNow,
                Status = "Running",
            };
            _dbContext.KfsScrubberUploadJobRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            // build task and queue
            _queue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
            {
                var dbContext = serviceProvider.GetRequiredService<SlothDbContext>();
                var kfsScrubberUploadJob = serviceProvider.GetRequiredService<KfsScrubberUploadJob>();

                // find job record
                var scopedRecord = await dbContext.KfsScrubberUploadJobRecords.FindAsync(record.Id);

                // build custom logger
                var log = LoggingConfiguration.GetJobConfiguration()
                    .CreateLogger()
                    .ForContext("jobname", scopedRecord.Name)
                    .ForContext("jobid", scopedRecord.Id);

                try
                {
                    // schedule methods
                    log.Information("Starting Job");
                    await kfsScrubberUploadJob.UploadScrubber(log);
                }
                finally
                {
                    // record status
                    scopedRecord.Status = "Finished";
                    await dbContext.SaveChangesAsync(token);
                }
            });

            return RedirectToAction(nameof(KfsScrubberUploadDetails), new { id = record.Id });
        }

        [HttpPost]
        public async Task<IActionResult> RunOneTimeCyberSourceIntegration(string integrationId, string reportName, DateTime date)
        {
            // fetch integration
            var integration = await _dbContext.Integrations
                .Include(i => i.Source)
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.Id == integrationId);

            // log run
            var record = new KfsScrubberUploadJobRecord()
            {
                Id     = Guid.NewGuid().ToString(),
                Name   = KfsScrubberUploadJob.JobName,
                RanOn  = DateTime.UtcNow,
                Status = "Running",
            };
            _dbContext.KfsScrubberUploadJobRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            // build custom logger
            var log = LoggingConfiguration.GetJobConfiguration()
                .CreateLogger()
                .ForContext("date", date)
                .ForContext("reportName", reportName)
                .ForContext("jobname", record.Name)
                .ForContext("jobid", record.Id);

            try
            {
                // schedule methods
                log.Information("Starting Job");

                await _cyberSourceBankReconcileService.ProcessOneTimeIntegration(integration, reportName, date, log);
            }
            finally
            {
                // record status
                record.Status = "Finished";
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(KfsScrubberUploadDetails), new { id = record.Id });
        }

        public async Task<IActionResult> CybersourceBankReconcile(CybersourceBankReconcileJobsFilterModel filter = null)
        {
            if (filter == null)
                filter = new CybersourceBankReconcileJobsFilterModel();

            SanitizeCybersourceBankReconcileJobsFilter(filter);

            var date = filter.Date ?? DateTime.Now.AddMonths(-1);

            var fromUtc = date.ToUniversalTime();
            var throughUtc = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)).ToUniversalTime();

            var result = new CybersourceBankReconcileJobsViewModel()
            {
                Filter = filter,
                Jobs = await _dbContext.CybersourceBankReconcileJobRecords
                    .Where(r => r.ProcessedDate > fromUtc && r.ProcessedDate <= throughUtc)
                    .OrderBy(r => r.ProcessedDate)
                    .ToListAsync()
        };

            return View(result);
        }

        public async Task<IActionResult> CybersourceBankReconcileDetails(string id)
        {
            var record = await _dbContext.CybersourceBankReconcileJobRecords
                .Include(r => r.Logs)
                .FirstOrDefaultAsync(r => r.Id == id);

            return View(record);
        }

        [HttpPost]
        public async Task<IActionResult> RunCybersourceBankReconcile(DateTime date)
        {
            // log run
            var record = new CybersourceBankReconcileJobRecord()
            {
                Id            = Guid.NewGuid().ToString(),
                Name          = CybersourceBankReconcileJob.JobName,
                RanOn         = DateTime.UtcNow,
                Status        = "Running",
                ProcessedDate = date,
            };
            _dbContext.CybersourceBankReconcileJobRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            // build task and queue
            _queue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
            {
                var dbContext = serviceProvider.GetRequiredService<SlothDbContext>();
                var cybersourceBankReconcileJob = serviceProvider.GetRequiredService<CybersourceBankReconcileJob>();

                // find job record
                var scopedRecord = await dbContext.CybersourceBankReconcileJobRecords.FindAsync(record.Id);

                // build custom logger
                var log = LoggingConfiguration.GetJobConfiguration()
                    .CreateLogger()
                    .ForContext("jobname", scopedRecord.Name)
                    .ForContext("jobid", scopedRecord.Id);

                try
                {
                    // call methods
                    log.Information("Starting Job");
                    await cybersourceBankReconcileJob.ProcessReconcile(date, log);
                }
                finally
                {
                    // record status
                    scopedRecord.Status = "Finished";
                    await dbContext.SaveChangesAsync(token);
                }
            });

            return RedirectToAction(nameof(CybersourceBankReconcileDetails), new { id = record.Id });
        }

        private static void SanitizeCybersourceBankReconcileJobsFilter(CybersourceBankReconcileJobsFilterModel model)
        {
            var date = (model.Date ?? DateTime.Now).Date;

            model.Date = new DateTime(date.Year, date.Month, 1);
        }
    }
}
