using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Core.Models
{
    public abstract class JobRecordBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        [Display(Name = "Ran On")]
        public DateTime RanOn { get; set; }

        public string Status { get; set; }

        public IList<LogMessage> Logs { get; set; }
    }
}
