using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Authorization;

namespace Sloth.Web.Handlers
{
    public class VerifyTeamPermissionHandler : AuthorizationHandler<VerifyTeamPermission>
    {
        private readonly UserManager<User> _userManager;

        public VerifyTeamPermissionHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyTeamPermission requirement)
        {
            if (context.User.IsInRole(Roles.SystemAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            var team = "";
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                if (mvcContext.RouteData.Values["team"] != null)
                {
                    team = mvcContext.RouteData.Values["team"].ToString();
                }
            }

            var user = await _userManager.GetUserAsync(context.User);
            if (user != null && team != "")
            {
                var permissions = user.UserTeamRoles
                    .Where(p => p.Team.Slug == team)
                    .Where(p => requirement.Roles.Contains(p.Role.Name));

                if (permissions.Any())
                {
                    context.Succeed(requirement);
                }
            }

            // TODO: Check for system admin role
        }
    }
}
