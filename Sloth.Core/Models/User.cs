using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Core.Models
{
    public class User
    {
        public User()
        {
            Keys = new List<ApiKey>();
        }

        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public IList<ApiKey> Keys { get; set; }
    }
}
