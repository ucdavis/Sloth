using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Identity;

namespace Sloth.Web.Controllers
{
    [Authorize]
    public abstract class SuperController : Controller
    {
        protected readonly ApplicationUserManager UserManager;
        protected readonly SlothDbContext DbContext;

        protected SuperController(ApplicationUserManager userManager, SlothDbContext dbContext)
        {
            UserManager = userManager;
            DbContext = dbContext;
        }

        [TempData(Key = "Message")]
        public string Message { get; set; }

        [TempData(Key = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        protected async Task<IReadOnlyList<Team>> GetUsersAdminTeams()
        {
            if (User.IsInRole(Roles.SystemAdmin))
            {
                // get all teams
                return await DbContext.Teams.ToListAsync();
            }

            // get user + roles, include their teams
            var userId = UserManager.GetUserId(User);
            var user = await DbContext.Users
                .Include(u => u.UserTeamRoles)
                    .ThenInclude(r => r.Team)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // select all teams that user is on
            return user.UserTeamRoles
                .Select(r => r.Team)
                .Distinct()
                .ToList();
        }

        protected async Task<string[]> GetRolesForTeam(string team) {
            var roles = await DbContext.UserTeamRoles
                .Where(p => p.Team.Slug == team).Select(a=>a.Role.Name).ToArrayAsync();

            return roles;
        }

        protected string TeamSlug => ControllerContext.RouteData.Values["team"] as string;
    }
}
