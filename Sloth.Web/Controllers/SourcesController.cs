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
using Sloth.Web.Models.SourceViewModels;

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
        public async Task<IActionResult> Create(string teamId)
        {
            ViewBag.Teams = await GetUsersAdminTeams();
            ViewBag.DefaultTeamId = teamId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSourceViewModel model)
        {
            var team = await DbContext.Teams.FirstOrDefaultAsync(t => t.Id == model.TeamId);
            if (team == null)
            {
                return View(model);
            }

            var source = new Source()
            {
                Name         = model.Name,
                Type         = model.Type,
                Description  = model.Description,
                OriginCode   = model.OriginCode,
                DocumentType = model.DocumentType,
                Team         = team
            };
            DbContext.Sources.Add(source);
            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var source = await DbContext.Sources
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (source == null)
            {
                return NotFound();
            }

            ViewBag.Teams = await GetUsersAdminTeams();

            var model = new EditSourceViewModel()
            {
                Name         = source.Name,
                TeamId       = source.Team.Id,
                Description  = source.Description,
                DocumentType = source.DocumentType,
                OriginCode   = source.OriginCode,
                Type         = source.Type,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditSourceViewModel model)
        {
            var source = await DbContext.Sources.FirstOrDefaultAsync(s => s.Id == id);

            if (source == null)
            {
                return NotFound();
            }

            var team = await DbContext.Teams.FirstOrDefaultAsync(t => t.Id == model.TeamId);
            if (team == null)
            {
                return View(model);
            }

            source.Name         = model.Name;
            source.Type         = model.Type;
            source.Description  = model.Description;
            source.OriginCode   = model.OriginCode;
            source.DocumentType = model.DocumentType;
            source.Team         = team;

            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
