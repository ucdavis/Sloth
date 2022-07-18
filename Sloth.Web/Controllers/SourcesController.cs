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

            // create new secret
            var secretId = Guid.NewGuid().ToString("D");
            await _secretsService.UpdateSecret(secretId, model.KfsFtpPasswordKey);

            var source = new Source()
            {
                Name                  = model.Name,
                Type                  = model.Type,
                Chart                 = model.Chart,
                OrganizationCode      = model.OrganizationCode,
                Description           = model.Description,
                OriginCode            = model.OriginCode,
                DocumentType          = model.DocumentType,
                KfsFtpUsername        = model.KfsFtpUsername,
                KfsFtpPasswordKeyName = secretId,
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
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (source == null)
            {
                return NotFound();
            }

            var model = new EditSourceViewModel()
            {
                Name                   = source.Name,
                Description            = source.Description,
                Chart                  = source.Chart,
                OrganizationCode       = source.OrganizationCode,
                DocumentType           = source.DocumentType,
                OriginCode             = source.OriginCode,
                Type                   = source.Type,
                KfsFtpUsername         = source.KfsFtpUsername,
                KfsFtpPasswordKeyDirty = false,
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

            // validate model
            var team = await DbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return View(model);
            }

            if (model.KfsFtpPasswordKeyDirty && string.IsNullOrWhiteSpace(model.KfsFtpPasswordKey))
            {
                return View(model);
            }

            // update source
            source.Name             = model.Name;
            source.Type             = model.Type;
            source.Description      = model.Description;
            source.Chart            = model.Chart;
            source.OrganizationCode = model.OrganizationCode;
            source.OriginCode       = model.OriginCode;
            source.DocumentType     = model.DocumentType;
            source.KfsFtpUsername   = model.KfsFtpUsername;
            source.Team             = team;

            // should we create a new secret?
            if (model.KfsFtpPasswordKeyDirty)
            {
                var secretId = Guid.NewGuid().ToString("D");
                await _secretsService.UpdateSecret(secretId, model.KfsFtpPasswordKey);
                source.KfsFtpPasswordKeyName = secretId;
            }

            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
