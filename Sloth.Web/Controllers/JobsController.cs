using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core;
using Sloth.Core.Jobs;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Logging;

namespace Sloth.Web.Controllers
{
    [Authorize(Roles = Roles.SystemAdmin)]
    public class JobsController : Controller
    {
        private readonly SlothDbContext _dbContext;
        private readonly CybersourceBankReconcileJob _cybersourceBankReconcileJob;

        public JobsController(SlothDbContext dbContext, CybersourceBankReconcileJob cybersourceBankReconcileJob)
        {
            _dbContext = dbContext;
            _cybersourceBankReconcileJob = cybersourceBankReconcileJob;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CybersourceBankReconcile()
        {
            var records = await _dbContext.CybersourceBankReconcileJobRecords
                .ToListAsync();

            return View(records);
        }

        public async Task<IActionResult> CybersourceBankReconcileDetails(string id)
        {
            var record = await _dbContext.CybersourceBankReconcileJobRecords
                .Include(r => r.Logs)
                .FirstOrDefaultAsync(r => r.Id == id);

            return View(record);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public async Task<IActionResult> CybersourceBankReconcileRecords(double from, double to, int utc_offset_from, int utc_offset_to)
        {
            // js offsets are in minutes
            var startOffset = new TimeSpan(0, utc_offset_from, 0);
            var endOffset = new TimeSpan(0, utc_offset_to, 0);

            // js ticks are in milliseconds, remove offset to utc
            var start = new DateTime(1970, 1, 1).AddMilliseconds(from) - startOffset;
            var end = new DateTime(1970, 1, 1).AddMilliseconds(to) - endOffset;

            // fetch records
            var records = await _dbContext.CybersourceBankReconcileJobRecords
                .Where(r => r.ProcessedDate >= start && r.ProcessedDate <= end)
                .ToListAsync();

            // js epoch is 1/1/1970
            var jsEpoch = new DateTime(1970, 1, 1).Ticks / 10_000;

            // format for js, add offset to local, reset to js epoch
            var events = records.Select(r => new
            {
                id = r.Id,
                title = r.Name,
                @class = "event-success",
                url = Url.Action(nameof(CybersourceBankReconcileDetails), new { id = r.Id }),
                start = (r.ProcessedDate.Ticks / 10_000) + (startOffset.Ticks / 10_000) - jsEpoch,
                end   = (r.ProcessedDate.Ticks / 10_000) + (startOffset.Ticks / 10_000) - jsEpoch + 1,
            });


            return new JsonResult(new
            {
                success = 1,
                result = events,
            });
        }

        [HttpPost]
        public async Task<IActionResult> RunCybersourceBankReconcile(DateTime date)
        {
            // log run
            var jobRecord = new CybersourceBankReconcileJobRecord()
            {
                Id            = Guid.NewGuid().ToString(),
                Name          = CybersourceBankReconcileJob.JobName,
                RanOn         = DateTime.UtcNow,
                Status        = "Running",
                ProcessedDate = date,
            };
            _dbContext.CybersourceBankReconcileJobRecords.Add(jobRecord);
            await _dbContext.SaveChangesAsync();

            // build custom logger
            var log = LoggingConfiguration.GetJobConfiguration()
                .CreateLogger()
                .ForContext("jobname", jobRecord.Name)
                .ForContext("jobid", jobRecord.Id);

            try
            {
                // call methods
                log.Information("Starting Job");
                await _cybersourceBankReconcileJob.ProcessReconcile(log, date);
            }
            finally
            {
                // record status
                jobRecord.Status = "Finished";
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(CybersourceBankReconcileDetails), new { id = jobRecord.Id });
        }
    }
}
