using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Sloth.Web.Logging
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
            var user = context.User.Identity.Name;

            using (_logger.BeginScope(new Dictionary<string, object>()
            {
                { "User", user }
            }))
            {
                await _next(context);
            }
        }
    }
}
