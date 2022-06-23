using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Authorization;

namespace Sloth.Web.Handlers
{
    public class VerifyTeamPermissionHandler : AuthorizationHandler<VerifyTeamPermission>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContext;

        public VerifyTeamPermissionHandler(UserManager<User> userManager, IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _httpContext = httpContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyTeamPermission requirement)
        {
            if (context.User.IsInRole(Roles.SystemAdmin))
            {
                // sys admin gets all available roles
                _httpContext.HttpContext.Items.Add("TeamRoles", new [] { TeamRole.Admin, TeamRole.Approver });

                context.Succeed(requirement);
                return;
            }

            // get team name from url
            var team = "";
            if (_httpContext.HttpContext.Request != null)
            {
                team = _httpContext.HttpContext.Request.RouteValues["team"]?.ToString() ?? "";
            }    

            var user = await _userManager.GetUserAsync(context.User);
            if (user != null && team != "")
            {
                var allTeamPermissions = user.UserTeamRoles.Where(p => p.Team.Slug == team);

                var permissions = allTeamPermissions.Where(p => requirement.Roles.Contains(p.Role.Name));

                if (permissions.Any())
                {
                    _httpContext.HttpContext.Items.Add("TeamRoles", allTeamPermissions.Select(a=>a.Role.Name).ToArray());

                    context.Succeed(requirement);
                }
            }
        }
    }
}
