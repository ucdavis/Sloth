using System;
using System.Collections.Generic;
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
        private readonly UserManager<User> _userManager;
        private readonly SlothDbContext _context;
        private readonly ISecretsService _secretsService;

        public IntegrationsController(UserManager<User> userManager, SlothDbContext context, ISecretsService secretsService)
        {
            _userManager = userManager;
            _context = context;
            _secretsService = secretsService;
        } 

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Teams = await GetUsersAdminTeams();

            ViewBag.Sources = await _context.Sources.ToListAsync();

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

            var source = await _context.Sources.FirstOrDefaultAsync(s => s.Id == integration.SourceId);
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
            _context.Integrations.Add(target);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = team.Id });
        }

        private async Task<IEnumerable<Team>> GetUsersAdminTeams()
        {
            // fetch user from db
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.UserTeamRoles)
                .ThenInclude(r => r.Team)
                .ThenInclude(t => t.Integrations)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // admin role
            var admin = await _context.Roles
                .FirstAsync(r => r.Name == Roles.Admin);

            // select teams where user is an admin
            var roles = user.UserTeamRoles.Where(r => r.RoleId == admin.Id);
            var teams = roles.Select(r => r.Team);

            return teams;
        }
    }
}
