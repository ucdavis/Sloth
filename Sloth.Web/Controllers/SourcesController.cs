using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Models.SourceViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamAdmin)]
    public class SourcesController : SuperController
    {
        private readonly ISecretsService _secretsService;

        public SourcesController(ApplicationUserManager userManager, SlothDbContext dbContext, ISecretsService secretsService) : base(userManager, dbContext)
        {
            _secretsService = secretsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sources = await DbContext.Sources
                .Include(s => s.Team)
                .Where(s => s.Team.Slug == TeamSlug)
                .ToListAsync();

            return View(sources);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSourceViewModel model)
        {
            var team = await DbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return View(model);
            }
            model.Name = model.Name.Trim();

            // validate model
            var isUnique = await DbContext.Sources
                .AllAsync(s => s.Name.ToLower() != model.Name.ToLower());

            if (!isUnique)
            {
                ModelState.AddModelError(nameof(model.Name), "Name must be unique across all teams.");
                return View(model);
            }

            var source = new Source()
            {
                Name                  = model.Name,
                Type                  = model.Type,
                Description           = model.Description,
                Team                  = team
            };
            DbContext.Sources.Add(source);
            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var source = await DbContext.Sources
                .Where(s => s.Team.Slug == TeamSlug)
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (source == null)
            {
                ErrorMessage = "Source not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditSourceViewModel()
            {
                Name                   = source.Name,
                Description            = source.Description,
                Type                   = source.Type,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditSourceViewModel model)
        {
            var source = await DbContext.Sources
                .Where(s => s.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (source == null)
            {
                ErrorMessage = "Source not found.";
                return RedirectToAction(nameof(Index));
            }

            // validate model
            var team = await DbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return View(model);
            }

            model.Name = model.Name.Trim();

            // validate model
            var isUnique = await DbContext.Sources
                .Where(a => a.Id != source.Id)
                .AllAsync(s => s.Name.ToLower() != model.Name.ToLower());

            if (!isUnique)
            {
                ModelState.AddModelError(nameof(model.Name), "Name must be unique across all teams.");
                return View(model);
            }

            // update source
            source.Name             = model.Name;
            source.Type             = model.Type;
            source.Description      = model.Description;
            source.Team             = team;

            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
