using System;
using System.Collections.Generic;
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
using Sloth.Web.Models.TransactionViewModels;
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

        [HttpGet("/jobs/{jobName}")]
        public async Task<IActionResult> JobList(string jobName, [FromQuery] JobsFilterModel filter = null)
        {
            if (filter == null)
            {
                filter = new JobsFilterModel();
            }
            SanitizeJobsFilter(filter);

            var fromUtc = filter.From.Value.ToUniversalTime();
            var throughUtc = filter.To.Value.AddDays(1).ToUniversalTime();

            var isOldCybersourceJob = jobName == CybersourceBankReconcileJob.JobName;
            var isOldWebhookJob = jobName == ResendPendingWebHookRequestsJob.JobName;
            var isOldKfsJob = jobName == KfsScrubberUploadJob.JobName;

            var jobs = jobName switch
            {
                CybersourceBankReconcileJob.JobName =>
                    await _dbContext.CybersourceBankReconcileJobRecords
                        .Where(r =>
                            ((r.ProcessedDate > fromUtc && r.ProcessedDate <= throughUtc)
                            || (r.RanOn > fromUtc && r.RanOn <= throughUtc))
                            && (!filter.HasTransactions || r.Transactions.Count > 0))
                        .OrderBy(r => r.ProcessedDate)
                        .ThenBy(r => r.RanOn)
                        .Select(r => new JobViewModel
                        {
                            Id = r.Id,
                            StartedAt = r.RanOn,
                            Status = r.Status,
                            Name = r.Name,
                            //TransactionCount = r.Transactions.Count
                        })
                        .ToListAsync(),
                KfsScrubberUploadJob.JobName =>
                    await _dbContext.KfsScrubberUploadJobRecords
                        .Where(r => r.RanOn > fromUtc && r.RanOn <= throughUtc
                                                    && (!filter.HasTransactions || r.Transactions.Count > 0))
                        .OrderBy(j => j.RanOn)
                        .Select(r => new JobViewModel
                        {
                            Id = r.Id,
                            StartedAt = r.RanOn,
                            Status = r.Status,
                            Name = r.Name,
                            //TransactionCount = r.Transactions.Count
                        })
                        .ToListAsync(),
                ResendPendingWebHookRequestsJob.JobName =>
                    await _dbContext.WebHookRequestResendJobRecords
                        .Where(r => r.RanOn > fromUtc && r.RanOn <= throughUtc)
                        .OrderBy(r => r.RanOn)
                        .Select(r => new JobViewModel
                        {
                            Id = r.Id,
                            StartedAt = r.RanOn,
                            Status = r.Status,
                            Name = r.Name,
                        })
                        .ToListAsync(),
                _ =>
                    await _dbContext.JobRecords
                        .Where(j =>
                            (string.IsNullOrEmpty(jobName) || j.Name == jobName)
                            && j.StartedAt >= fromUtc
                            && j.StartedAt < throughUtc)
                        .OrderBy(j => j.StartedAt)
                        .Select(j => new JobViewModel
                        {
                            Id = j.Id,
                            Name = j.Name,
                            StartedAt = j.StartedAt,
                            EndedAt = j.EndedAt,
                            Status = j.Status,
                            Details = j.Details,
                        })
                        .ToListAsync()
            };

            var result = new JobListViewModel()
            {
                Filter = filter,
                Jobs = jobs,
                JobName = jobName,
            };

            return View(result);
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

            var fromUtc = filter.From.Value.ToUniversalTime();
            var throughUtc = filter.To.Value.AddDays(1).ToUniversalTime();

            var result = new KfsScrubberJobsViewModel()
            {
                Filter = filter,
                Jobs = await _dbContext.KfsScrubberUploadJobRecords
                    .Where(r => r.RanOn > fromUtc && r.RanOn <= throughUtc
                                                  && (!filter.HasTransactions || r.Transactions.Count > 0))
                    .OrderBy(j => j.RanOn)
                    .Select(r => new KfsScrubberJobViewModel
                    {
                        Job = r,
                        TransactionCount = r.Transactions.Count
                    })
                    .ToListAsync()
            };

            return View(result);
        }

        public async Task<IActionResult> KfsScrubberUploadDetails(string id)
        {
            var record = await _dbContext.KfsScrubberUploadJobRecords
                .Where(r => r.Id == id)
                .Include(r => r.Logs)
                .Include(r => r.Transactions)
                .ThenInclude(t => t.Transfers)
                .Select(r => new KfsScrubberJobViewModel()
                {
                    Job = r,
                    TransactionsTable = new TransactionsTableViewModel()
                    {
                        Transactions = r.Transactions
                    },
                    TransactionCount = r.Transactions.Count
                })
                .FirstOrDefaultAsync();

            return View(record);
        }

        public Task<IActionResult> WebhookResendDetails(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> JobDetails(string id)
        {
            throw new NotImplementedException();
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

                var jobBlob = await _cyberSourceBankReconcileService.ProcessOneTimeIntegration(integration, reportName, date, record, log);
                if (jobBlob != null)
                {
                    // save uploaded blob metadata
                    _dbContext.CybersourceBankReconcileJobBlobs.Add(jobBlob);
                }
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

            var fromUtc = filter.From.Value.ToUniversalTime();
            var throughUtc = filter.To.Value.AddDays(1).ToUniversalTime();

            var result = new CybersourceBankReconcileJobsViewModel()
            {
                Filter = filter,
                Jobs = await _dbContext.CybersourceBankReconcileJobRecords
                    .Where(r =>
                        ((r.ProcessedDate > fromUtc && r.ProcessedDate <= throughUtc)
                        || (r.RanOn > fromUtc && r.RanOn <= throughUtc))
                        && (!filter.HasTransactions || r.Transactions.Count > 0))
                    .OrderBy(r => r.ProcessedDate)
                    .ThenBy(r => r.RanOn)
                    .Select(r => new CybersourceBankReconcileJobViewModel
                    {
                        Job = r,
                        TransactionCount = r.Transactions.Count
                    })
                    .ToListAsync()
            };

            return View(result);
        }

        public async Task<IActionResult> CybersourceBankReconcileDetails(string id)
        {
            var viewModel = await _dbContext.CybersourceBankReconcileJobRecords
                .Where(r => r.Id == id)
                .Include(r => r.Logs)
                .Include(r => r.Transactions)
                .ThenInclude(t => t.Transfers)
                .Include(a => a.Transactions)
                .ThenInclude(a => a.Source)
                .ThenInclude(a => a.Team)
                .AsNoTracking()
                .Select(r => new CybersourceBankReconcileJobViewModel
                {
                    Job = r,
                    TransactionsTable = new TransactionsTableViewModel()
                    {
                        Transactions = r.Transactions
                    },
                    TransactionCount = r.Transactions.Count
                })
                .FirstOrDefaultAsync();

            return View(viewModel);
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
                    await foreach (var jobBlob in cybersourceBankReconcileJob.ProcessReconcile(date, record, log))
                    {
                        if (jobBlob == null) continue;
                        // save uploaded blob metadata
                        dbContext.CybersourceBankReconcileJobBlobs.Add(jobBlob);
                    }
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
            model.From = (model.From ?? DateTime.Today).Date.AddMonths(-1);
            model.To = (model.To ?? DateTime.Today).Date;
        }
    }
}
