using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class User : IdentityUser
    {
        public User()
        {
            Keys = new List<ApiKey>();
        }

        [JsonIgnore]
        public IList<ApiKey> Keys { get; set; }
    }
}
