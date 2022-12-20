using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Web.Identity;
using Sloth.Web.Models.ScrubberViewModels;
using Sloth.Web.Models.TransactionViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamAnyRole)]
    public class ScrubbersController : SuperController
    {
        public ScrubbersController(ApplicationUserManager userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        public async Task<IActionResult> Index()
        {
            var scrubbers = await DbContext.Scrubbers
                .Include(s => s.Source)
                    .ThenInclude(s => s.Team)
                .Where(s => s.Source.Team.Slug == TeamSlug)
                .AsNoTracking()
                .ToListAsync();

            return View(scrubbers);
        }

        public async Task<IActionResult> Details(string id)
        {
            var scrubber = await DbContext.Scrubbers
                .Where(s => s.Source.Team.Slug == TeamSlug)
                .Include(s => s.Source)
                    .ThenInclude(s => s.Team)
                .Include(s => s.Transactions)
                    .ThenInclude(s => s.Source)
                .Include(s => s.Transactions)
                    .ThenInclude(t => t.Transfers)
                .Include(s => s.Transactions)
                    .ThenInclude(t => t.ReversalOfTransaction)
                        .ThenInclude(r => r.Source)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (scrubber == null)
            {
                ErrorMessage = "Scrubber not found.";
                return RedirectToAction(nameof(Index));
            }

            var scrubberDetails = new ScrubberDetailsViewModel()
            {
                Scrubber = scrubber,
                TransactionsTable = new TransactionsTableViewModel()
                {
                    Transactions = scrubber.Transactions
                }
            };

            return View(scrubberDetails);
        }
    }
}
