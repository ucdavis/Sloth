using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Core.Models
{
    public abstract class JobRecord
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Ran On")]
        public DateTime RanOn { get; set; }

        public string Status { get; set; }

        public IList<LogMessage> Logs { get; set; }
    }
}
