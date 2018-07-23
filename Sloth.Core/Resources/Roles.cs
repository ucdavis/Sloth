using System;

namespace Sloth.Core.Resources
{
    public class Roles
    {
        // Role Codes
        public const string SystemAdmin = "SystemAdmin";

        public  static string[] GetAllRoles()
        {
            return new[]
            {
                SystemAdmin,
            };
        }
    }
}
