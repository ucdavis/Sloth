using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            // Policy names map to scopes
            var requiredScopes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy)
                .Distinct()
                .ToList();

            if (!requiredScopes.Any()) return;

            operation.Responses.Add("401", new Response { Description = "Unauthorized" });
            operation.Responses.Add("403", new Response { Description = "Forbidden" });

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
            operation.Security.Add(new Dictionary<string, IEnumerable<string>>
            {
                { "apiKey", requiredScopes }
            });


            //var requiredClaimTypes = requiredScopes
            //    .Select(x => _authorizationOptions.Value.GetPolicy(x))
            //    .SelectMany(x => x.Requirements)
            //    .OfType<ClaimsAuthorizationRequirement>()
            //    .Select(x => x.ClaimType)
            //    .ToList();

            //if (!requiredClaimTypes.Any()) return;

            //operation.Responses.Add("401", new Response { Description = "Unauthorized" });
            //operation.Responses.Add("403", new Response { Description = "Forbidden" });

            //operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            //{
            //    new Dictionary<string, IEnumerable<string>>
            //    {
            //        { "apiKey", requiredClaimTypes }
            //    }
            //};
        }
    }

}
