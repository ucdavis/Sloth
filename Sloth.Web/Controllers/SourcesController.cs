using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public SourcesController(UserManager<User> userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sources = await DbContext.Sources
                .Include(s => s.Team)
                .ToListAsync();

            return View(sources);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Teams = DbContext.Teams.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSourceViewModel source)
        {
            var team = await DbContext.Teams.FirstOrDefaultAsync(t => t.Id == source.TeamId);
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
            DbContext.Sources.Add(target);
            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
