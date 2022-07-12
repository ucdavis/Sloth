using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sloth.Core;

namespace Sloth.Api.Identity
{
    public class ApiKeyMiddleware
    {
        public const string HeaderKey = "X-Auth-Token";

        private readonly RequestDelegate _next;
        private readonly ApiKeyProviderOptions _options;
        private ILogger _logger;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyProviderOptions> options, ILoggerFactory loggerFactory)
        {
            _next = next;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<ApiKeyMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context, SlothDbContext dbContext)
        {
            // check for header
            if (!context.Request.Headers.ContainsKey(HeaderKey))
            {
                await _next(context);
                return;
            }
            var headerValue = context.Request.Headers[HeaderKey].FirstOrDefault();

            // lookup apikey from db
            var apiKey = await dbContext.ApiKeys
                .Include(a => a.Team)
                .FirstOrDefaultAsync(a => a.Key == headerValue);

            if (apiKey == null || apiKey.Revoked.HasValue)
            {
                await _next(context);
                return;
            }

            context.User.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, apiKey.Team.Name),
                new Claim(ClaimTypes.PrimaryGroupSid, apiKey.Team.Id)
            }));

            await _next(context);
        }
    }

    public class ApiKeyProviderOptions
    {
    }
}
