using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Core.Resources
{
    public class Roles
    {
        // System Roles
        public static string SystemAdmin = "SystemAdmin";

        // Team Roles
        public static string Admin = "Admin";
        public static string Approver = "Approver";

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
