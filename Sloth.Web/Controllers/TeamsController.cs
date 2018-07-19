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
        private readonly UserManager<User> _userManager;
        private readonly SlothDbContext _context;

        public TeamsController(UserManager<User> userManager, SlothDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Team> teams;
            if (User.IsInRole(Roles.SystemAdmin))
            {
                // get all teams
                teams = _context.Teams.ToList();
            }
            else
            {
                // get user + roles, include their teams
                var userId = _userManager.GetUserId(User);
                var user = await _context.Users
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
            var team = await _context.Teams
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
            var teamRoles = new[] { Roles.Admin, Roles.Approver };
            ViewBag.Roles = await _context.Roles
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
            var user = await _userManager.GetUserAsync(User);

            // fetch admin role
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == Roles.Admin);

            var team = new Team()
            {
                Name = model.Name,
            };
            team.AddUserToRole(user, adminRole);

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = team.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserRole(string teamId, string userId, string roleId)
        {
            // fetch team from db
            var team = await _context.Teams
                .Include(t => t.ApiKeys)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            // find user or create them
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            // find role
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);

            team.AddUserToRole(user, role);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewApiKey(string teamId)
        {
            // fetch team from db
            var team = await _context.Teams
                .Include(t => t.ApiKeys)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            // create new key
            var apiKey = new ApiKey();

            // associate key and update db
            team.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

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
            var team = await _context.Teams
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
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                id = apiKey.Id,
                revoked = apiKey.Revoked.Value.ToPacificTime().ToString("g"),
            });
        }
    }
}
