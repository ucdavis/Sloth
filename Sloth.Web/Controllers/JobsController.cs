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
using Sloth.Core.abstractions;
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
            var jobModel = await _dbContext.JobRecords
                .Where(r => r.Id == id)
                .Select(r => new JobDetailsViewModel
                {
                    Job = new JobViewModel()
                    {
                        Id = r.Id,
                        Name = r.Name,
                        StartedAt = r.StartedAt,
                        EndedAt = r.EndedAt,
                        Status = r.Status,
                        Details = r.Details,
                        TransactionCount = r.TotalTransactions ?? 0
                    }
                })
                .SingleOrDefaultAsync();

            if (jobModel == null)
            {
                return NotFound();
            }

            var detailsJson = jobModel.Job.Details as string;

            if (!string.IsNullOrWhiteSpace(detailsJson))
            {
                object details = null;
                switch (jobModel.Job.Name)
                {
                    case CybersourceBankReconcileJob.JobName:
                        details = JsonSerializer.Deserialize<CybersourceBankReconcileDetails>(detailsJson);
                        jobModel.Job.Details = details;
                        break;
                    case KfsScrubberUploadJob.JobName:
                        var scrubberUploadDetails = JsonSerializer.Deserialize<KfsScrubberUploadJob.KfsScrubberUploadJobDetails>(detailsJson);
                        jobModel.Job.Details = scrubberUploadDetails;
                        var scrubberIds = scrubberUploadDetails.TransactionGroups.Select(g => g.ScrubberId).ToArray();
                        if (scrubberIds.Length > 0)
                        {
                            var transactions = await _dbContext.Transactions
                                .Where(t => scrubberIds.Contains(t.ScrubberId))
                                .Include(t => t.Transfers)
                                .Include(a => a.Source)
                                    .ThenInclude(a => a.Team)
                                .AsNoTracking()
                                .ToListAsync();
                            jobModel.TransactionsTable = new TransactionsTableViewModel
                            {
                                Transactions = transactions
                            };
                        }
                        break;
                    case AggieEnterpriseJournalJob.JobNameUploadTransactions:
                    case AggieEnterpriseJournalJob.JobNameResolveProcessingJournals:
                        details = JsonSerializer.Deserialize<AggieEnterpriseJournalJob.AggieEnterpriseJournalJobDetails>(detailsJson);
                        jobModel.Job.Details = details;
                        break;
                    case ResendPendingWebHookRequestsJob.JobName:
                        details = JsonSerializer.Deserialize<ResendPendingWebHookRequestsJob.WebHookRequestJobDetails>(detailsJson);
                        jobModel.Job.Details = details;
                        break;
                }

                if (jobModel.TransactionsTable == null && details is IHasTransactionIds detailsWithTransactionIds)
                {
                    var transactionIds = detailsWithTransactionIds.GetTransactionIds().ToArray();
                    if (transactionIds.Length > 0)
                    {
                        var transactions = await _dbContext.Transactions
                            .Where(t => transactionIds.Contains(t.Id))
                            .Include(t => t.Transfers)
                            .Include(a => a.Source)
                                .ThenInclude(a => a.Team)
                            .AsNoTracking()
                            .ToListAsync();
                        jobModel.TransactionsTable = new TransactionsTableViewModel
                        {
                            Transactions = transactions
                        };
                    }
                }

            }

            return View(jobModel);
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
            var jobRecord = new JobRecord()
            {
                Name = CybersourceBankReconcileJob.JobName,
                StartedAt = DateTime.UtcNow,
                Status = JobRecord.Statuses.Running,
                ProcessedDate = date
            };
            _dbContext.JobRecords.Add(jobRecord);
            await _dbContext.SaveChangesAsync();

            // build custom logger
            var log = LoggingConfiguration.GetJobConfiguration()
                .CreateLogger()
                .ForContext("date", date)
                .ForContext("reportName", reportName)
                .ForContext("jobname", jobRecord.Name)
                .ForContext("jobid", jobRecord.Id);

            var reconcileDetails = new CybersourceBankReconcileDetails();
            try
            {
                // schedule methods
                log.Information("Starting Job");

                reconcileDetails.IntegrationDetails.Add(
                    await _cyberSourceBankReconcileService.ProcessOneTimeIntegration(integration, reportName, date, log));
                // record status
                jobRecord.SetCompleted(JobRecord.Statuses.Finished, reconcileDetails);
            }
            catch(Exception ex)
            {
                log.Error("Unexpected error processing integration", ex);
                jobRecord.SetCompleted(JobRecord.Statuses.Failed, new());
            }
            finally
            {
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = jobRecord.Id });
        }

        [HttpPost]
        public async Task<IActionResult> RunJob(JobRunRequestModel jobRunRequest)
        {
            // log run
            var record = new JobRecord()
            {
                Name = jobRunRequest.JobName,
                StartedAt = DateTime.UtcNow,
                Status = JobRecord.Statuses.Running,
                ProcessedDate = jobRunRequest.Date,
            };
            _dbContext.JobRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            // build task and queue
            _queue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
            {
                var dbContext = serviceProvider.GetRequiredService<SlothDbContext>();

                // find job record
                var scopedRecord = await dbContext.JobRecords.FindAsync(record.Id);

                // build custom logger
                var log = LoggingConfiguration.GetJobConfiguration()
                    .CreateLogger()
                    .ForContext("jobname", scopedRecord.Name)
                    .ForContext("jobid", scopedRecord.Id);

                object jobDetails = null;
                try
                {
                    // schedule methods
                    log.Information("Starting Job");
                    switch (scopedRecord.Name)
                    {
                        case KfsScrubberUploadJob.JobName:
                            var kfsScrubberUploadJob = serviceProvider.GetRequiredService<KfsScrubberUploadJob>();
                            jobDetails = await kfsScrubberUploadJob.UploadScrubber(log);
                            break;
                        case CybersourceBankReconcileJob.JobName:
                            var cybersourceBankReconcileJob = serviceProvider.GetRequiredService<CybersourceBankReconcileJob>();
                            jobDetails = await cybersourceBankReconcileJob.ProcessReconcile(jobRunRequest.Date.Value, log);
                            break;
                        case AggieEnterpriseJournalJob.JobNameResolveProcessingJournals:
                            var aeJournalJob = serviceProvider.GetRequiredService<AggieEnterpriseJournalJob>();
                            jobDetails = await aeJournalJob.ResolveProcessingJournals(log);
                            break;
                        case AggieEnterpriseJournalJob.JobNameUploadTransactions:
                            aeJournalJob = serviceProvider.GetRequiredService<AggieEnterpriseJournalJob>();
                            jobDetails = await aeJournalJob.UploadTransactions(log);
                            break;
                        case ResendPendingWebHookRequestsJob.JobName:
                            var resendWebhookJob = serviceProvider.GetRequiredService<ResendPendingWebHookRequestsJob>();
                            jobDetails = await resendWebhookJob.ResendPendingWebHookRequests();
                            break;
                    }
                }
                finally
                {
                    // record status
                    scopedRecord.SetCompleted(JobRecord.Statuses.Finished, jobDetails ?? new());
                    await dbContext.SaveChangesAsync(token);
                }
            });

            return RedirectToAction(nameof(Details), new { id = record.Id });
        }

        private static void SanitizeJobsFilter(JobsFilterModel model)
        {
            model.From = (model.From ?? DateTime.Today).Date.AddMonths(-1);
            model.To = (model.To ?? DateTime.Today).Date;
        }
    }
}
