using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;

namespace Sloth.Web.Controllers
{
    [Authorize]
    public abstract class SuperController : Controller
    {
        protected readonly UserManager<User> UserManager;
        protected readonly SlothDbContext DbContext;

        protected SuperController(UserManager<User> userManager, SlothDbContext dbContext)
        {
            UserManager = userManager;
            DbContext = dbContext;
        }

        protected async Task<IEnumerable<Team>> GetUsersAdminTeams()
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

        protected string TeamSlug => ControllerContext.RouteData.Values["team"] as string;
        }
    }
