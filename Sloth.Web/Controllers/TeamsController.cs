using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Models.TeamViewModels;

namespace Sloth.Web.Controllers
{
    public class TeamsController : SuperController
    {
        public TeamsController(UserManager<User> userManager, SlothDbContext dbContext) : base (userManager, dbContext)
        {
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Team> teams;
            if (User.IsInRole(Roles.SystemAdmin))
            {
                // get all teams
                teams = DbContext.Teams.ToList();
            }
            else
            {
                // get user + roles, include their teams
                var userId = UserManager.GetUserId(User);
                var user = await DbContext.Users
                    .Include(u => u.UserTeamRoles)
                        .ThenInclude(r => r.Team)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                // select all teams that user is on
                teams = user.UserTeamRoles
                    .Select(r => r.Team)
                    .Distinct();
            }

            return View(teams);
        }

        public async Task<IActionResult> Details(string id)
        {
            var team = await DbContext.Teams
                .Include(t => t.ApiKeys)
                .Include(t => t.Integrations)
                .Include(t => t.UserTeamRoles)
                    .ThenInclude(r => r.User)
                .Include(t => t.UserTeamRoles)
                    .ThenInclude(r => r.Role)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            // fetch team roles
            var teamRoles = new[] { TeamRole.Admin, TeamRole.Approver };
            ViewBag.Roles = await DbContext.TeamRoles
                .Where(r => teamRoles.Contains(r.Name))
                .ToListAsync();

            return View(team);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            // fetch user
            var user = await UserManager.GetUserAsync(User);

            // fetch admin role
            var adminRole = await DbContext.TeamRoles.FirstAsync(r => r.Name == TeamRole.Admin);

            var team = new Team()
            {
                Name = model.Name,
            };
            team.AddUserToRole(user, adminRole);

            DbContext.Teams.Add(team);
            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = team.Id });
        }

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
            });
        }

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
