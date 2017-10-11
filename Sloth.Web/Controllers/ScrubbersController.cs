using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;

namespace Sloth.Web.Controllers
{
    public class ScrubbersController : SuperController
    {
        private readonly SlothDbContext _context;

        public ScrubbersController(SlothDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var scrubbers = await _context.Scrubbers
                .AsNoTracking()
                .ToListAsync();

            return View(scrubbers);
        }

        public async Task<IActionResult> Details(string id)
        {
            var scrubber = await _context.Scrubbers
                .Include(t => t.Transactions)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            return View(scrubber);
        }
    }
}
