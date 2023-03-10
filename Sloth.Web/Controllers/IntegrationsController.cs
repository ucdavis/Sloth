using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Models.IntegrationViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamAdmin)]
    public class IntegrationsController : SuperController
    {
        private readonly ISecretsService _secretsService;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public IntegrationsController(ApplicationUserManager userManager, SlothDbContext dbContext, ISecretsService secretsService,
            IAggieEnterpriseService aggieEnterpriseService)
            : base(userManager, dbContext)
        {
            _secretsService = secretsService;
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var integration = await DbContext.Integrations
                .Where(i => i.Source.Team.Slug == TeamSlug)
                .Include(i => i.Source)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (integration == null)
            {
                ErrorMessage = "Integration not found";
                return RedirectToAction(nameof(Index));
            }

            return View(integration);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Sources = await DbContext.Sources
                .Where(s => s.Team.Slug == TeamSlug)
                .ToListAsync();

            ViewBag.Types = new[]
            {
                IntegrationTypes.CyberSource
            };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIntegrationViewModel model)
        {
            // TODO: validate model
            var adminTeams = await GetUsersAdminTeams();
            var team = adminTeams.FirstOrDefault(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return View(model);
            }

            var source = await DbContext.Sources.FirstOrDefaultAsync(s => s.Id == model.SourceId);
            if (source == null)
            {
                return View(model);
            }

            // create new secret
            var secretId = Guid.NewGuid().ToString("D");
            await _secretsService.UpdateSecret(secretId, model.ReportPassword);

            // create integration
            var target = new Integration
            {
                MerchantId        = model.MerchantId,
                Source            = source,
                Team              = team,
                Type              = model.Type,
                ReportUsername    = model.ReportUserName,
                ReportPasswordKey = secretId,
                ClearingAccount   = model.ClearingAccount,
                HoldingAccount    = model.HoldingAccount,
            };
            DbContext.Integrations.Add(target);
            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = team.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.Sources = await DbContext.Sources.ToListAsync();

            ViewBag.Types = new[]
            {
                IntegrationTypes.CyberSource
            };

            var integration = await DbContext.Integrations
                .Where(i => i.Source.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (integration == null)
            {
                ErrorMessage = "Integration not found";
                return RedirectToAction(nameof(HomeController.TeamIndex), "Home", new { team = TeamSlug });
            }

            var model = new EditIntegrationViewModel()
            {
                ClearingAccount     = integration.ClearingAccount,
                HoldingAccount      = integration.HoldingAccount,
                MerchantId          = integration.MerchantId,
                ReportPasswordDirty = false,
                SourceId            = integration.Source.Id,
                ReportUserName      = integration.ReportUsername,
                Type                = integration.Type,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditIntegrationViewModel model)
        {
            var integration = await DbContext.Integrations
                .Where(i => i.Source.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (integration == null)
            {
                ErrorMessage = "Integration not found";
                return RedirectToAction(nameof(HomeController.TeamIndex), "Home", new { team = TeamSlug });
            }

            // validate model
            var adminTeams = await GetUsersAdminTeams();
            var team = adminTeams.FirstOrDefault(t => t.Slug == TeamSlug);
            if (team == null)
            {
                ErrorMessage = "Team not found";
                return RedirectToAction("Index", "Home");
            }

            var source = await DbContext.Sources.FirstOrDefaultAsync(s => s.Id == model.SourceId);
            if (source == null)
            {
                ViewBag.ErrorMessage = "Validation Failed";
                ModelState.AddModelError(nameof(model.SourceId), "Source not found");
            }

            if (model.ReportPasswordDirty && string.IsNullOrWhiteSpace(model.ReportPassword))
            {
                ViewBag.ErrorMessage = "Validation Failed";
                ModelState.AddModelError(nameof(model.ReportPassword), "Password is required");
            }

            if (!await _aggieEnterpriseService.IsAccountValid(model.ClearingAccount, true))
            {
                ViewBag.ErrorMessage = "Validation Failed";
                ModelState.AddModelError(nameof(model.ClearingAccount), "Invalid Clearing Account");
            }

            if (!await _aggieEnterpriseService.IsAccountValid(model.HoldingAccount, true))
            {
                ViewBag.ErrorMessage = "Validation Failed";
                ModelState.AddModelError(nameof(model.HoldingAccount), "Invalid Holding Account");
            }

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                ViewBag.Sources = await DbContext.Sources.ToListAsync();
                ViewBag.Types = new[]
                {
                    IntegrationTypes.CyberSource
                };
                return View(model);
            }

            // update integration
            integration.MerchantId      = model.MerchantId;
            integration.Source          = source;
            integration.Team            = team;
            integration.Type            = model.Type;
            integration.ReportUsername  = model.ReportUserName;
            integration.ClearingAccount = model.ClearingAccount;
            integration.HoldingAccount  = model.HoldingAccount;

            // should we create a new secret?
            if (model.ReportPasswordDirty)
            {
                // ensure the secret is base64 encoded, because reasons
                var base64Password = model.ReportPassword.IsBase64() ? model.ReportPassword : model.ReportPassword.Base64Encode();
                var secretId = Guid.NewGuid().ToString("D");
                await _secretsService.UpdateSecret(secretId, base64Password);
                integration.ReportPasswordKey = secretId;
            }

            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = team!.Id });
        }
    }
}
