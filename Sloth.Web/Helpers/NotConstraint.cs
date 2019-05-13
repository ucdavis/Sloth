using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Sloth.Web.Helpers
{
    public class NotConstraint : IRouteConstraint
    {
        private readonly IRouteConstraint _constraint;

        public NotConstraint(string constraint)
        {
            _constraint = new RegexInlineRouteConstraint(constraint);
        }

        public NotConstraint(IRouteConstraint constraint)
        {
            _constraint = constraint;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            return !_constraint.Match(httpContext, route, routeKey, values, routeDirection);
        }
    }
}
