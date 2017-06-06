using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Sloth.Api.Identity
{
    public class ApiKeyHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
