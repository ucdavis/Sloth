using System;

namespace Sloth.Core.Resources
{
    public class Roles
    {
        // System Roles
        public const string SystemAdmin = "SystemAdmin";

        // Team Roles
        public const string Admin = "Admin";
        public const string Approver = "Approver";

        internal static string[] GetAllRoles()
        {
            return new[]
            {
                SystemAdmin,
                Admin,
                Approver,
            };
        }
    }
}
