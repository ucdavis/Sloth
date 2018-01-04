using System;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Core.Models
{
    public class Integration
    {
        public string Id { get; set; }

        [Required]
        public Team Team { get; set; }

        [Required]
        public Source Source { get; set; }

        [Required]
        public string Type { get; set; }

        public string MerchantId { get; set; }

        public string ReportUsername { get; set; }

        public string ReportPasswordKey { get; set; }

        public string DefaultAccount { get; set; }
    }
}
