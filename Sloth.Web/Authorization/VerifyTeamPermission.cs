using System;
using Microsoft.AspNetCore.Authorization;

namespace Sloth.Web.Authorization
{
    public class VerifyTeamPermission : IAuthorizationRequirement
    {
        public readonly object[] Roles;

        public VerifyTeamPermission(params object[] roles)
        {
            Roles = roles;
        }
    }
}
