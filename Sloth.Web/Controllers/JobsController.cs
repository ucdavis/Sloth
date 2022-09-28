using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
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

            var jobs = jobName switch
            {
                CybersourceBankReconcileJob.JobName =>
                    await _dbContext.JobRecords
                        .Where(j =>
                            j.Name == CybersourceBankReconcileJob.JobName
                            && ((j.ProcessedDate > fromUtc && j.ProcessedDate <= throughUtc)
                                || (j.StartedAt > fromUtc && j.StartedAt <= throughUtc))
                            && (!filter.HasTransactions || j.TotalTransactions > 0))
                        .OrderBy(r => r.ProcessedDate)
                        .ThenBy(r => r.StartedAt)
                        .Select(r => new JobViewModel
                        {
                            Id = r.Id,
                            StartedAt = r.StartedAt,
                            EndedAt = r.EndedAt,
                            ProcessedDate = r.ProcessedDate,
                            Status = r.Status,
                            Name = r.Name,
                            TransactionCount = r.TotalTransactions ?? 0
                        })
                        .ToListAsync(),
                _ =>
                    await _dbContext.JobRecords
                        .Where(j =>
                            j.Name == jobName
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
                            //Details = j.Details,
                            TransactionCount = j.TotalTransactions ?? 0
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

        public async Task<IActionResult> Details(string id)
        {
            var record = await _dbContext.JobRecords
                .Where(r => r.Id == id)
                .Select(r => new JobViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    StartedAt = r.StartedAt,
                    EndedAt = r.EndedAt,
                    Status = r.Status,
                    Details = r.Details,
                    TransactionCount = r.TotalTransactions ?? 0
                })
                .FirstOrDefaultAsync();

            return View(record);
        }

        [HttpPost]
        public async Task<IActionResult> RunKfsScrubberUpload()
        {
            // log run
            var record = new JobRecord()
            {
                Name = KfsScrubberUploadJob.JobName,
                StartedAt = DateTime.UtcNow,
                Status = "Running",
            };
            _dbContext.JobRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            // build task and queue
            _queue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
            {
                var dbContext = serviceProvider.GetRequiredService<SlothDbContext>();
                var kfsScrubberUploadJob = serviceProvider.GetRequiredService<KfsScrubberUploadJob>();

                // find job record
                var scopedRecord = await dbContext.JobRecords.FindAsync(record.Id);

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

            return RedirectToAction(nameof(Details), new { id = record.Id });
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
            var record = new JobRecord()
            {
                Name = CybersourceBankReconcileJob.JobName,
                StartedAt = DateTime.UtcNow,
                Status = "Running",
                ProcessedDate = date
            };
            _dbContext.JobRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            // build custom logger
            var log = LoggingConfiguration.GetJobConfiguration()
                .CreateLogger()
                .ForContext("date", date)
                .ForContext("reportName", reportName)
                .ForContext("jobname", record.Name)
                .ForContext("jobid", record.Id);

            var reconcileDetails = new CybersourceBankReconcileDetails();
            try
            {
                // schedule methods
                log.Information("Starting Job");

                reconcileDetails.IntegrationDetails.Add(
                    await _cyberSourceBankReconcileService.ProcessOneTimeIntegration(integration, reportName, date, log));
            }
            finally
            {
                // record status
                record.SetCompleted("Finished", reconcileDetails);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(CybersourceBankReconcileDetails), new { id = record.Id });
        }

        public async Task<IActionResult> CybersourceBankReconcileDetails(string jobRecordId)
        {
            var record = await _dbContext.JobRecords
                .Where(r => r.Id == jobRecordId && r.Name == CybersourceBankReconcileJob.JobName)
                .SingleOrDefaultAsync();
            if (record == null)
            {
                return NotFound();
            }
            var jobDetails = JsonSerializer.Deserialize<CybersourceBankReconcileDetails>(record.Details ?? "{}");
            var transactionIds = jobDetails.IntegrationDetails.SelectMany(i => i.TransactionIds).ToList();
            var transactions = await _dbContext.Transactions
                .Where(t => transactionIds.Contains(t.Id))
                .Include(t => t.Transfers)
                .Include(a => a.Source)
                .ThenInclude(a => a.Team)
                .AsNoTracking()
                .ToListAsync();
            var viewModel = new CybersourceBankReconcileJobViewModel
            {
                Job = record,
                TransactionsTable = new TransactionsTableViewModel()
                {
                    Transactions = transactions
                },
                TransactionCount = transactions.Count
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RunCybersourceBankReconcile(DateTime date)
        {
            // log run
            var record = new JobRecord()
            {
                Name = CybersourceBankReconcileJob.JobName,
                StartedAt = DateTime.UtcNow,
                Status = "Running",
                ProcessedDate = date,
            };
            _dbContext.JobRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            // build task and queue
            _queue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
            {
                var dbContext = serviceProvider.GetRequiredService<SlothDbContext>();
                var cybersourceBankReconcileJob = serviceProvider.GetRequiredService<CybersourceBankReconcileJob>();

                // find job record
                var scopedRecord = await dbContext.JobRecords.FindAsync(record.Id);

                // build custom logger
                var log = LoggingConfiguration.GetJobConfiguration()
                    .CreateLogger()
                    .ForContext("jobname", scopedRecord.Name)
                    .ForContext("jobid", scopedRecord.Id);
                var jobDetails = new CybersourceBankReconcileDetails();
                try
                {
                    // call methods
                    log.Information("Starting Job");
                    jobDetails = await cybersourceBankReconcileJob.ProcessReconcile(date, log);
                }
                finally
                {
                    // record status
                    scopedRecord.SetCompleted("Finished", jobDetails);
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
