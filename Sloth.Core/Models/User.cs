using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class User : IdentityUser<Guid>
    {
        public User()
        {
            Keys = new List<ApiKey>();
        }

        public IList<ApiKey> Keys { get; set; }
    }
}
