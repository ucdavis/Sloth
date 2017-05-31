using System;
using Microsoft.AspNetCore.Mvc;
using Sloth.Api.Resources;

namespace Sloth.Api.Attributes
{
    public class VersionedRoute : RouteAttribute
    {
        public VersionedRoute(string template) : base(PrependVersion(template))
        {
        }

        public VersionedRoute(string version, string template) : base(PrependVersion(version, template))
        {
        }

        private static string PrependVersion(string version, string template)
        {
            return "v" + version + "/" + template;
        }

        private static string PrependVersion(string template)
        {
            return PrependVersion(VersionString.Current, template);
        }
    }
}
