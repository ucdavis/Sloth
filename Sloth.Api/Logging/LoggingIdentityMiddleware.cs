using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Sloth.Api.Logging
{
    public class LoggingIdentityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LoggingIdentityMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<LoggingIdentityMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var claim = context.User.Identities.SelectMany(i => i.Claims)
                .Where(c => c.Value != null)
                .Select(c => c.Value)
                .FirstOrDefault();

            using (_logger.BeginScope(new Dictionary<string, object>()
            {
                { "User", $"Api-{claim}" }
            }))
            {
                await _next(context);
            }
        }
    }
}
