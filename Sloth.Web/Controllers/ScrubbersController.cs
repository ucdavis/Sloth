using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Web.Controllers
{
    public class ScrubbersController : SuperController
    {
        public ScrubbersController(UserManager<User> userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        public async Task<IActionResult> Index()
        {
            var scrubbers = await DbContext.Scrubbers
                .Include(s => s.Source)
                    .ThenInclude(s => s.Team)
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
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            return View(scrubber);
        }
    }
}
