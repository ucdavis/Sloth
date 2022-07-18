using System;

namespace Sloth.Core.Extensions
{
    public static class AggieEnterpriseExtensions
    {
        public static string StripToErpName(this string value, int maxLength = Int32.MaxValue)
        {
            // Erp names can only contain letters, numbers, hyphen, spaces and underscores.
            const string regexStr = @"[^0-9a-zA-Z\ \_\-\']+";

            return value.SafeTruncate(maxLength).SafeRegexRemove(regexStr);
        }

        public static string StripToErpDescription(this string value, int maxLength = Int32.MaxValue)
        {
            //  limited to letters, numbers, hyphen, underscore, spaces, and periods.
            const string regexStr = @"[^0-9a-zA-Z\ \.\-\_']+";

            return value.SafeTruncate(maxLength).SafeRegexRemove(regexStr);
        }

        public static string StripToGlReferenceField(this string value, int maxLength = Int32.MaxValue)
        {
            //  limited to letters, numbers, hyphen, and underscore
            const string regexStr = @"[^0-9a-zA-Z\-\_']+";

            return value.SafeTruncate(maxLength).SafeRegexRemove(regexStr);
        }
    }
}
