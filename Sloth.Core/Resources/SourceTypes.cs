using System;

namespace Sloth.Core.Resources
{
    public static class SourceTypes
    {
        public static string CyberSource = "CyberSource";

        public static string Recharge = "Recharge";

        public static string[] GetAll()
        {
            return new[]
            {
                CyberSource,
                Recharge,
            };
        }
    }
}
