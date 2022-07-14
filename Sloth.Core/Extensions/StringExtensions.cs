using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Sloth.Core.Extensions
{
    public static class StringExtensions
    {
        public static Stream GenerateStreamFromString(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string SafeTruncate(this string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= max)
            {
                return value;
            }

            if (max <= 0)
            {
                return String.Empty;
            }

            return value.Substring(0, max);
        }

        public static string SafeRegexRemove(this string value, string regEx = @"[^0-9a-zA-Z\.\-\' ]+")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            try
            {
                return Regex.Replace(value, regEx, string.Empty);
            }
            catch (Exception)
            {
                return value;
            }
        }

    }
}
