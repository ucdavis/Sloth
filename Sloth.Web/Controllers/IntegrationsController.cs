using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Models;

namespace Sloth.Web.Controllers
{
    public class IntegrationsController : SuperController
    {
        private readonly ISecretsService _secretsService;

        public IntegrationsController(UserManager<User> userManager, SlothDbContext dbContext, ISecretsService secretsService)
            : base(userManager, dbContext)
        {
            _secretsService = secretsService;
        } 

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Teams = await GetUsersAdminTeams();

            ViewBag.Sources = await DbContext.Sources.ToListAsync();

            ViewBag.Types = new[]
            {
                IntegrationTypes.CyberSource
            };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIntegrationViewModel integration)
        {
            // TODO: validate model
            var adminTeams = await GetUsersAdminTeams();
            var team = adminTeams.FirstOrDefault(t => t.Id == integration.TeamId);
            if (team == null)
            {
                return View();
            }

            var source = await DbContext.Sources.FirstOrDefaultAsync(s => s.Id == integration.SourceId);
            if (source == null)
            {
                return View();
            }

            // create new secret
            var secretId = Guid.NewGuid().ToString("D");
            await _secretsService.UpdateSecret(secretId, integration.ReportPassword);

            // create integration
            var target = new Integration
            {
                MerchantId        = integration.MerchantId,
                Source            = source,
                Team              = team,
                Type              = integration.Type,
                DefaultAccount    = integration.DefaultAccount,
                ReportUsername    = integration.ReportUserName,
                ReportPasswordKey = secretId
            };
            DbContext.Integrations.Add(target);
            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = team.Id });
        }
    }
}
