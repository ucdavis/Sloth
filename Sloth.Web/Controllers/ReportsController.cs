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

            var fiveDaysAgo = DateTime.UtcNow.Date.AddDays(-5);

            // get latest TransactionStatusEvent for each transaction
            // and filter that set to only rejected or processing older than 5 days
            var txnIdsFromStaleOrRejectedProcessingEvents = DbContext.TransactionStatusEvents
                .GroupBy(e => e.TransactionId)
                .Select(g => new
                {
                    TransactionId = g.Key,
                    MaxDate = g.Max(e => e.EventDate)
                })
                .Join(DbContext.TransactionStatusEvents,
                    outer => new { outer.TransactionId, EventDate = outer.MaxDate },
                    inner => new { inner.TransactionId, inner.EventDate },
                    (_, inner) => inner
                )
                .Where(e => e.Status == TransactionStatuses.Rejected
                    || (e.Status == TransactionStatuses.Processing && e.EventDate < fiveDaysAgo))
                .Select(e => e.TransactionId);

            var transactions = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug && txnIdsFromStaleOrRejectedProcessingEvents.Contains(t.Id))
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

        [Authorize(Roles = Roles.SystemAdmin)]
        public async Task<IActionResult> FailedTransactionsAllTeams()
        {
            var fiveDaysAgo = DateTime.UtcNow.Date.AddDays(-5);

            // get latest TransactionStatusEvent for each transaction
            // and filter that set to only rejected or processing older than 5 days
            var txnIdsFromStaleOrRejectedProcessingEvents = DbContext.TransactionStatusEvents
                .GroupBy(e => e.TransactionId)
                .Select(g => new
                {
                    TransactionId = g.Key,
                    MaxDate = g.Max(e => e.EventDate)
                })
                .Join(DbContext.TransactionStatusEvents,
                    outer => new { outer.TransactionId, EventDate = outer.MaxDate },
                    inner => new { inner.TransactionId, inner.EventDate },
                    (_, inner) => inner
                )
                .Where(e => e.Status == TransactionStatuses.Rejected
                    || (e.Status == TransactionStatuses.Processing && e.EventDate < fiveDaysAgo))
                .Select(e => e.TransactionId);

            var transactions = await DbContext.Transactions
                .Where(t => txnIdsFromStaleOrRejectedProcessingEvents.Contains(t.Id))
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
