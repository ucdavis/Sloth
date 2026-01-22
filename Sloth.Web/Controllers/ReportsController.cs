using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Models.BlobViewModels;
using Sloth.Web.Models.ReportViewModels;
using Sloth.Web.Models.TransactionViewModels;
using Sloth.Web.Resources;
using Sloth.Web.Helpers;
using Sloth.Web.Models;
using Microsoft.Extensions.Options;

namespace Sloth.Web.Controllers
{

    [Authorize(Policy = PolicyCodes.TeamAnyRole)]
    public class ReportsController : SuperController
    {
        private readonly DataLimitingOptions _dataLimitingOptions;
        public ReportsController(ApplicationUserManager userManager, SlothDbContext dbContext, IOptions<DataLimitingOptions> dataLimitingOptions) : base(userManager, dbContext)
        {
            _dataLimitingOptions = dataLimitingOptions.Value;
        }

        public IActionResult Index()
        {
            return View("Index", TeamSlug);
        }

        public async Task<IActionResult> FailedTransactions()
        {
            if (string.IsNullOrWhiteSpace(TeamSlug))
            {
                return BadRequest("TeamSlug is required");
            }

            // get transactions that are rejected or have been processing for longer than 5 days
            var transactions = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug
                    && (
                        t.Status == TransactionStatuses.Rejected
                        || (t.Status == TransactionStatuses.Processing && t.LastModified < DateTime.UtcNow.Date.AddDays(-5))
                    )
                )
                .Include(t => t.Transfers)
                .Include(a => a.JournalRequest)
                .AsNoTracking()
                .ToListAsync();

            var model = new TransactionsReportViewModel
            {
                TransactionsTable = new TransactionsTableViewModel
                {
                    Transactions = transactions,
                }
            };

            return View("FailedTransactions", model);
        }

        [Route("{team}/reports/downloadabletransactions/{filter?}")]
        [HttpGet]
        public async Task<IActionResult> DownloadableTransactions(TransactionsFilterModel filter = null)
        {
            if (string.IsNullOrWhiteSpace(TeamSlug))
            {
                return BadRequest("TeamSlug is required");
            }

            if (filter == null)
                filter = new TransactionsFilterModel();

            FilterHelpers.SanitizeTransactionsFilter(filter, _dataLimitingOptions.DefaultDateRange);

            var model = new TransfersReportViewModel
            {
                Filter = filter,
            };

            model.Transactions = await DbContext.Transactions.Include(a => a.Transfers).Include(a => a.Metadata)
                .Where(t => t.Source.Team.Slug == TeamSlug && t.TransactionDate >= filter.From && t.TransactionDate <= filter.To).OrderBy(a => a.Id).ThenBy(a => a.TransactionDate).Select(TransactionWithTransfers.Projection()).ToListAsync();

            var team = await DbContext.Teams.FirstAsync(t => t.Slug == TeamSlug);
            ViewBag.Title = $"Transactions with Transfers - {team.Name}";

            return View(model);
        }


        public async Task<IActionResult> StuckTransactions()
        {
            if (string.IsNullOrWhiteSpace(TeamSlug))
            {
                return BadRequest("TeamSlug is required");
            }

            // get transactions that are rejected or have been processing for longer than 5 days
            var query = DbContext.Transactions
                .Include(t => t.Transfers)
                .Include(a => a.JournalRequest)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .AsNoTracking();

                query = query.Where(t => t.Source.Team.Slug == TeamSlug &&
                    ((t.Status == TransactionStatuses.Processing && t.LastModified < DateTime.UtcNow.Date.AddDays(-1)) ||
                    (t.Status == TransactionStatuses.Scheduled && t.LastModified < DateTime.UtcNow.Date.AddDays(-1))))
                ;


            var transactions = await query.ToListAsync();

            var model = new TransactionsReportViewModel
            {
                TransactionsTable = new TransactionsTableViewModel
                {
                    Transactions = transactions,
                },
                Information = "Only transactions that have been processing or scheduled for more than a day!!!",
                StuckOnly = true,
            };

            return View("FailedTransactions", model);
        }

        [Authorize(Roles = Roles.SystemAdmin)]
        public async Task<IActionResult> FailedTransactionsAllTeams(bool pendingOlderThanADay = false)
        {
            // get transactions that are rejected or have been processing for longer than 5 days
            var query =  DbContext.Transactions
                .Include(t => t.Transfers)
                .Include(a => a.JournalRequest)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .AsNoTracking();

            if (pendingOlderThanADay)
            {
                query = query.Where(t =>
                    (t.Status == TransactionStatuses.Processing && t.LastModified < DateTime.UtcNow.Date.AddDays(-1)) ||
                    (t.Status == TransactionStatuses.Scheduled && t.LastModified < DateTime.UtcNow.Date.AddDays(-1)))
                ;
            }
            else
            {
                query = query.Where(t => t.Status == TransactionStatuses.Rejected
                    || (t.Status == TransactionStatuses.Processing && t.LastModified < DateTime.UtcNow.Date.AddDays(-5)));
            }

            var transactions = await query.ToListAsync();

            var model = new TransactionsReportViewModel
            {
                TransactionsTable = new TransactionsTableViewModel
                {
                    Transactions = transactions,
                },
                Information = pendingOlderThanADay ? "Only transactions that have been processing or scheduled for more than a day!!!" : null,
                StuckOnly = pendingOlderThanADay,
            };

            return View("FailedTransactions", model);
        }
    }
}
