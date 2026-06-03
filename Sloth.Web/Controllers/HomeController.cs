using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Resources;
using Sloth.Web.Identity;
using Sloth.Web.Models;
using Sloth.Web.Models.HomeViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    public class HomeController : SuperController
    {
        public HomeController(ApplicationUserManager userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        public async Task<IActionResult> Index()
        {
            // look for users teams, redirect if there's only one
            var teams = await GetUsersAdminTeams();

            if (teams.Count == 1)
            {
                var team = teams[0];
                return RedirectToAction(nameof(TransactionsController.Index), "Transactions", new { team = team.Slug });
            }

            var teamIds = teams.Select(t => t.Id).ToList();
            var teamSlugs = teams.Select(t => t.Slug).ToList();
            var stuckCutoff = DateTime.UtcNow.Date.AddDays(-1);

            var teamsWithSources = await DbContext.Teams
                .Include(t => t.Sources)
                .Where(t => teamIds.Contains(t.Id))
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();

            var failedTransactionCounts = await DbContext.Transactions
                .Where(t => teamSlugs.Contains(t.Source.Team.Slug)
                    && t.Status == TransactionStatuses.Rejected)
                .GroupBy(t => t.Source.Team.Slug)
                .Select(t => new
                {
                    Slug = t.Key,
                    Count = t.Count(),
                })
                .ToDictionaryAsync(t => t.Slug, t => t.Count);

            var stuckTransactionCounts = await DbContext.Transactions
                .Where(t => teamSlugs.Contains(t.Source.Team.Slug)
                    && ((t.Status == TransactionStatuses.Processing && t.LastModified < stuckCutoff)
                        || (t.Status == TransactionStatuses.Scheduled && t.LastModified < stuckCutoff)))
                .GroupBy(t => t.Source.Team.Slug)
                .Select(t => new
                {
                    Slug = t.Key,
                    Count = t.Count(),
                })
                .ToDictionaryAsync(t => t.Slug, t => t.Count);

            var model = new HomeIndexViewModel
            {
                Teams = teamsWithSources
                    .Select(t => new HomeTeamSummaryViewModel
                    {
                        Name = t.Name,
                        Slug = t.Slug,
                        SourceNames = t.Sources?.OrderBy(s => s.Name).Select(s => s.Name).ToList() ?? new List<string>(),
                        FailedTransactionCount = failedTransactionCounts.TryGetValue(t.Slug, out var failedCount) ? failedCount : 0,
                        StuckTransactionCount = stuckTransactionCounts.TryGetValue(t.Slug, out var stuckCount) ? stuckCount : 0,
                    })
                    .ToList(),
            };

            return View(model);
        }

        [Authorize(Policy = PolicyCodes.TeamAnyRole)]
        public async Task<IActionResult> TeamIndex()
        {
            // find team
            var team = await DbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return NotFound();
            }

            // check for transactions need approval
            var needApproval = await DbContext.Transactions
                .Include(t => t.Transfers)
                .Where(t => t.Status == TransactionStatuses.PendingApproval)
                .Where(t => t.Source.Team.Slug == TeamSlug)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.NeedApproval = needApproval;

            return View();
        }

        [AllowAnonymous]
        public IActionResult LoggedOut() {
            return Content("You have been successfully logged out.  You can now close this window.");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
