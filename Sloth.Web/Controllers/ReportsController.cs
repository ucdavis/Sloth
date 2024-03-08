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

namespace Sloth.Web.Controllers
{

    [Authorize(Policy = PolicyCodes.TeamAnyRole)]
    public class ReportsController : SuperController
    {
        public ReportsController(ApplicationUserManager userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        { }

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

        public async Task<IActionResult> DownloadableTransactions(TransactionsFilterModel filter = null)
        {
            if (string.IsNullOrWhiteSpace(TeamSlug))
            {
                return BadRequest("TeamSlug is required");
            }

            if (filter == null)
                filter = new TransactionsFilterModel();

            filter.From = filter.From = new DateTime(2023, 10, 12);
            filter.To = filter.To = new DateTime(2023, 10, 15);

            FilterHelpers.SanitizeTransactionsFilter(filter);

            var model = new TransfersReportViewModel
            {
                filter = filter,
            };

            model.Transactions = await DbContext.Transactions.Include(a => a.Transfers).Include(a => a.Metadata)
                .Where(t => t.Source.Team.Slug == TeamSlug && t.TransactionDate >= filter.From && t.TransactionDate <= filter.To).Select(TransactionWithTransfers.Projection()).ToListAsync();

            return View(model);
        }

        [Authorize(Roles = Roles.SystemAdmin)]
        public async Task<IActionResult> FailedTransactionsAllTeams()
        {
            // get transactions that are rejected or have been processing for longer than 5 days
            var transactions = await DbContext.Transactions
                .Where(t => t.Status == TransactionStatuses.Rejected
                    || (t.Status == TransactionStatuses.Processing && t.LastModified < DateTime.UtcNow.Date.AddDays(-5)))
                .Include(t => t.Transfers)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
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
    }
}
