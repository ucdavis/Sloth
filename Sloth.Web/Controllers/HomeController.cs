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

            return View(teams);
        }

        [Authorize(Policy = PolicyCodes.TeamApprover)]
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
