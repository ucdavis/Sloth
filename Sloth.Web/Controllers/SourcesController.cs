using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Models;

namespace Sloth.Web.Controllers
{
    [Authorize(Roles = Roles.SystemAdmin)]
    public class SourcesController : SuperController
    {
        private readonly SlothDbContext _context;

        public SourcesController(SlothDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sources = await _context.Sources
                .Include(s => s.Team)
                .ToListAsync();

            return View(sources);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Teams = _context.Teams.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSource source)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == source.TeamId);
            if (team == null)
            {
                return View();
            }

            var target = new Source()
            {
                Name = source.Name,
                Type = source.Type,
                Description = source.Description,
                OriginCode = source.OriginCode,
                DocumentType = source.DocumentType,
                Team = team
            };
            _context.Sources.Add(target);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
