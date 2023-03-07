using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Web.Identity;
using Sloth.Web.Models.TeamViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    public class TeamsController : SuperController
    {
        public TeamsController(ApplicationUserManager userManager, SlothDbContext dbContext) : base (userManager, dbContext)
        {
        }

        public async Task<IActionResult> Index()
        {
            var teams = await GetUsersAdminTeams();
            return View(teams);
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> Details()
        {
            var team = await DbContext.Teams
                .Include(t => t.ApiKeys)
                .Include(t => t.Integrations)
                .Include(t => t.Sources)
                .Include(t => t.UserTeamRoles)
                    .ThenInclude(r => r.User)
                .Include(t => t.UserTeamRoles)
                    .ThenInclude(r => r.Role)
                .Include(t => t.WebHooks)
                .FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                return NotFound();
            }

            // fetch team roles
            var teamRoles = new[] { TeamRole.Admin, TeamRole.Manager, TeamRole.Approver };
            ViewBag.Roles = await DbContext.TeamRoles
                .Where(r => teamRoles.Contains(r.Name))
                .ToListAsync();

            return View(team);
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            // fetch user
            var user = await UserManager.GetUserAsync(User);

            // fetch admin role
            var adminRole = await DbContext.TeamRoles.FirstAsync(r => r.Name == TeamRole.Admin);

            var team = new Team()
            {
                Name                     = model.Name,
                Slug                     = model.Slug,
                KfsContactDepartmentName = model.KfsContactDepartmentName,
                KfsContactUserId         = model.KfsContactUserId,
                KfsContactEmail          = model.KfsContactEmail,
                KfsContactPhoneNumber    = model.KfsContactPhoneNumber,
                KfsContactMailingAddress = model.KfsContactMailingAddress,
            };
            team.AddUserToRole(user, adminRole);

            DbContext.Teams.Add(team);
            await DbContext.SaveChangesAsync();

            return RedirectToAction("TeamIndex", "Home", new { team = team.Slug });
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var teams = await GetUsersAdminTeams();
            var team = teams.FirstOrDefault(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return NotFound();
            }

            var model = new EditTeamViewModel()
            {
                Name                     = team.Name,
                Slug                     = team.Slug,
                KfsContactUserId         = team.KfsContactUserId,
                KfsContactDepartmentName = team.KfsContactDepartmentName,
                KfsContactEmail          = team.KfsContactEmail,
                KfsContactPhoneNumber    = team.KfsContactPhoneNumber,
                KfsContactMailingAddress = team.KfsContactMailingAddress,
            };

            return View(model);
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpPost]
        public async Task<IActionResult> Edit(EditTeamViewModel model)
        {
            var teams = await GetUsersAdminTeams();
            var team = teams.FirstOrDefault(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return NotFound();
            }

            team.Name                     = model.Name;
            team.Slug                     = model.Slug;
            team.KfsContactDepartmentName = model.KfsContactDepartmentName;
            team.KfsContactUserId         = model.KfsContactUserId;
            team.KfsContactEmail          = model.KfsContactEmail;
            team.KfsContactPhoneNumber    = model.KfsContactPhoneNumber;
            team.KfsContactMailingAddress = model.KfsContactMailingAddress;

            await DbContext.SaveChangesAsync();
            return RedirectToAction("Details", new { id = team.Id });
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpPost]
        public async Task<IActionResult> CreateUserRole(string teamId, string userId, string roleId)
        {
            // fetch team from db
            var team = await DbContext.Teams
                .Include(t => t.ApiKeys)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            // find user
            var user = await DbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            // find role
            var role = await DbContext.TeamRoles
                .FirstOrDefaultAsync(r => r.Id == roleId);

            team.AddUserToRole(user, role);
            await DbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true
            });
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string teamId, string userId, string roleId)
        {

            // fetch team from db
            var team = await DbContext.Teams
                .Include(a => a.UserTeamRoles)
                .ThenInclude(a => a.Role)
                .SingleAsync(t => t.Id == teamId && t.Slug == TeamSlug);


            // find user
            var user = await DbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            var utr = team.UserTeamRoles.SingleOrDefault(a => a.UserId == userId && a.RoleId == roleId);

            if (utr == null)
            {
                return NotFound();
            }

            team.UserTeamRoles.Remove(utr);

            await DbContext.SaveChangesAsync();

            Log.Warning($"Team User Role removed: team: {team.Name} user: {user.UserName} role: {utr.Role.Name}");

            return RedirectToAction("Details", new { id = team.Id });
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpPost]
        public async Task<IActionResult> CreateNewApiKey(string teamId)
        {
            // fetch team from db
            var team = await DbContext.Teams
                .Include(t => t.ApiKeys)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            // create new key
            var apiKey = new ApiKey();

            // associate key and update db
            team.ApiKeys.Add(apiKey);
            await DbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                id = apiKey.Id,
                key = apiKey.Key,
                issued = apiKey.Issued.ToPacificTime().ToString("g"),
                issuedTicks = apiKey.Issued.Ticks,
            });
        }

        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        [HttpPost]
        public async Task<IActionResult> RevokeApiKey(string id, string teamId)
        {
            // fetch team from db
            var team = await DbContext.Teams
                .Include(t => t.ApiKeys)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            // find key on team
            var apiKey = team.ApiKeys.FirstOrDefault(k => k.Id == id);
            if (apiKey == null)
            {
                return NotFound();
            }

            // set revoke on key
            apiKey.Revoked = DateTime.UtcNow;
            await DbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                id = apiKey.Id,
                revoked = apiKey.Revoked.Value.ToPacificTime().ToString("g"),
            });
        }
    }
}
