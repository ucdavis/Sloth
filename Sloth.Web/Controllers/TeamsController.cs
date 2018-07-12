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
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.UserTeamRoles)
                    .ThenInclude(r => r.Team)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // teams
            var teams = user.UserTeamRoles.Select(r => r.Team);

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

            return View(team);
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
