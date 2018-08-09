using System;
// ReSharper disable InconsistentNaming

namespace Sloth.Core.Resources
{
    public class DocumentTypes
    {
        public const string GLIB = "GLIB";
        public const string GLJV = "GLJV";
        public const string GLCC = "GLCC";
        public const string GLJB = "GLJB";
        public const string GLBB = "GLBB";
        public const string GLCB = "GLCB";
        public const string GLDE = "GLDE";

        public static string[] GetAll()
        {
            return new[]
            {
                GLIB,
                GLJV,
                GLCC,
                GLJB,
                GLBB,
                GLCB,
                GLDE,
            };
        }

        public static string GetDocumentTypeFilePrefix(string documentType)
        {
            switch (documentType)
            {
                case GLIB:
                    return "billing";

                case GLJV:
                    return "journal";
                
                default:
                    return "";
            }
        }
    }
}
