using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Sloth.Web.Helpers
{
    public static class RoleHelpers {
        public static bool IsInTeamRole(this HttpContext context, string role) {
            var roles = context.Items["TeamRoles"] as string[];

            if (roles == null) {
                return false;
            }

            return roles.Contains(role);
        } 
    }
}
