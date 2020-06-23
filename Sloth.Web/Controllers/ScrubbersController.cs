using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Web.Identity;
using Sloth.Web.Models.ScrubberViewModels;
using Sloth.Web.Models.TransactionViewModels;

namespace Sloth.Web.Controllers
{
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

            var scrubberDetails = new ScrubberDetailsViewModel()
            {
                Scrubber = scrubber,
                TransactionsTable = new TransactionsTableViewModel()
                {
                    Transactions = scrubber.Transactions,
                    HasWebhooks = await DbContext.WebHooks
                        .AnyAsync(w => w.Team.Slug == TeamSlug)
                }
            };

            return View(scrubberDetails);
        }
    }
}
