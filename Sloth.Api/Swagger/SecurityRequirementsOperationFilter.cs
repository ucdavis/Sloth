using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sloth.Api.Swagger
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        private readonly IOptions<AuthorizationOptions> _authorizationOptions;

        public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
        {
            this._authorizationOptions = authorizationOptions;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerPolicies = context.ApiDescription.ControllerAttributes()
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);

            var actionPolicies = context.ApiDescription.ActionAttributes()
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);

            var policies = controllerPolicies.Union(actionPolicies).Distinct();

            var requiredClaimTypes = policies
                .Select(x => _authorizationOptions.Value.GetPolicy(x))
                .SelectMany(x => x.Requirements)
                .OfType<ClaimsAuthorizationRequirement>()
                .Select(x => x.ClaimType)
                .ToList();

            if (!requiredClaimTypes.Any()) return;

            operation.Responses.Add("401", new Response { Description = "Unauthorized" });
            operation.Responses.Add("403", new Response { Description = "Forbidden" });

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            {
                new Dictionary<string, IEnumerable<string>>
                {
                    { "apiKey", requiredClaimTypes }
                }
            };
        }
    }

}
