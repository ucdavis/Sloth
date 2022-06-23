using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Sloth.Web.Helpers
{
    public static class RoleHelpers
    {
        public static bool HasTeamRole(this HttpContext context, params string[] roles)
        {
            var teamRoles = context.Items["TeamRoles"] as string[];

            if (teamRoles == null || roles == null)
            {
                return false;
            }

            return teamRoles.Any(r => roles.Contains(r));
        }
    }
}
