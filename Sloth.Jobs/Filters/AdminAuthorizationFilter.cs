using System;
using Hangfire.Dashboard;

namespace Sloth.Jobs.Filters
{
    public class AdminAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var user = httpContext.User;
            return user.Identity.IsAuthenticated;
        }
    }
}
