using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Sloth.Core.Models
{
    public class User : IdentityUser
    {
        public User()
        {
            Keys = new List<ApiKey>();
        }

        public IList<ApiKey> Keys { get; set; }
    }
}
