using System;
// ReSharper disable InconsistentNaming

namespace Sloth.Core.Resources
{
    public class DocumentTypes
    {
        public static string GLIB = "GLIB";
        public static string GLJV = "GLJV";
        public static string GLCC = "GLCC";
        public static string GLJB = "GLJB";
        public static string GLBB = "GLBB";
        public static string GLCB = "GLCB";
        public static string GLDE = "GLDE";

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
    }
}
