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
                .AsNoTracking()
                .ToListAsync();

            return View(scrubbers);
        }

        public async Task<IActionResult> Details(string id)
        {
            var scrubber = await DbContext.Scrubbers
                .Include(s => s.Source)
                .Include(t => t.Transactions)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            return View(scrubber);
        }
    }
}
