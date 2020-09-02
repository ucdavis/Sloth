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

        public async Task<IActionResult> KfsScrubberUpload(JobsFilterModel filter)
        {
            if (filter == null)
                filter = new JobsFilterModel();

            SanitizeJobsFilter(filter);

            var date = filter.Date ?? DateTime.Now.AddMonths(-1);

            var fromUtc = date.ToUniversalTime();
            var throughUtc = date.AddMonths(1).ToUniversalTime();

            var result = new KfsScrubberJobsViewModel()
            {
                Filter = filter,
                Jobs = await _dbContext.KfsScrubberUploadJobRecords
                    .Where(r => r.RanOn > fromUtc && r.RanOn <= throughUtc)
                    .OrderBy(j => j.RanOn)
                    .ToListAsync()
            };

            return View(result);
        }

        public async Task<IActionResult> KfsScrubberUploadDetails(string id)
        {
            var record = await _dbContext.KfsScrubberUploadJobRecords
                .Include(r => r.Logs)
                .FirstOrDefaultAsync(r => r.Id == id);

            return View(record);
        }

        [HttpPost]
        public async Task<IActionResult> RunKfsScrubberUpload()
        {
            // log run
            var record = new KfsScrubberUploadJobRecord()
            {
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
                    await kfsScrubberUploadJob.UploadScrubber(log, record);
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
            var record = new CybersourceBankReconcileJobRecord()
            {
                Name   = CybersourceBankReconcileJob.JobName,
                RanOn  = DateTime.UtcNow,
                Status = "Running",
                ProcessedDate = date
            };
            _dbContext.CybersourceBankReconcileJobRecords.Add(record);
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

                await _cyberSourceBankReconcileService.ProcessOneTimeIntegration(integration, reportName, date, record, log);
            }
            finally
            {
                // record status
                record.Status = "Finished";
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(CybersourceBankReconcileDetails), new { id = record.Id });
        }

        public async Task<IActionResult> CybersourceBankReconcile(JobsFilterModel filter = null)
        {
            if (filter == null)
                filter = new JobsFilterModel();

            SanitizeJobsFilter(filter);

            var date = filter.Date ?? DateTime.Now.AddMonths(-1);

            var fromUtc = date.ToUniversalTime();
            var throughUtc = date.AddMonths(1).ToUniversalTime();

            var result = new CybersourceBankReconcileJobsViewModel()
            {
                Filter = filter,
                Jobs = await _dbContext.CybersourceBankReconcileJobRecords
                    .Where(r =>
                        (r.ProcessedDate > fromUtc && r.ProcessedDate <= throughUtc)
                        || (r.RanOn > fromUtc && r.RanOn <= throughUtc))
                    .OrderBy(r => r.ProcessedDate)
                    .ThenBy(r => r.RanOn)
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
                    await cybersourceBankReconcileJob.ProcessReconcile(date, record, log);
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

        private static void SanitizeJobsFilter(JobsFilterModel model)
        {
            var date = (model.Date ?? DateTime.Now).Date;

            model.Date = new DateTime(date.Year, date.Month, 1);
        }
    }
}
