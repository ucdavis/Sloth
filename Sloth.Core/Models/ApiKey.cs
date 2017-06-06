using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Core.Models
{
    public class ApiKey
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public DateTime Issued { get; set; }
        public DateTime? Revoked { get; set; }
    }
}
