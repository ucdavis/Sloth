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
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        public VerifyTeamPermissionHandler(UserManager<User> userManager, IHttpContextAccessor httpContext, ITempDataDictionaryFactory tempDataDictionary)
        {
            _userManager = userManager;
            _httpContext = httpContext;
            _tempDataDictionaryFactory = tempDataDictionary;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyTeamPermission requirement)
        {
            if (context.User.IsInRole(Roles.SystemAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // get team name from url
            var team = "";
            if (context.Resource is Endpoint)
            {
                team = _httpContext.HttpContext.Request.RouteValues["team"]?.ToString() ?? "";
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
