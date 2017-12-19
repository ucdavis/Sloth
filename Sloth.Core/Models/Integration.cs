using System;

namespace Sloth.Core.Models
{
    public class Integration
    {
        public string Id { get; set; }

        public string TeamId { get; set; }

        public Team Team { get; set; }

        public IntegrationType Type { get; set; }

        public string MerchantId { get; set; }

        public string ReportUsername { get; set; }

        public string ReportPasswordKey { get; set; }

        public string DefaultAccount { get; set; }

        public enum IntegrationType
        {
            CyberSource
        }
    }
}
