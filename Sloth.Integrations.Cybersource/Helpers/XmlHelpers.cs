using System;
using System.Text.RegularExpressions;

namespace Sloth.Integrations.Cybersource.Helpers
{
    public static class XmlHelpers
    {
        public static string RemoveAllNamespaces(string xml)
        {
            const string pattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
            return Regex.Replace(xml, pattern, "");
        }
    }
}
