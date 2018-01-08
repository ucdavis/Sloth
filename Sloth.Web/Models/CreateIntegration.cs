namespace Sloth.Web.Models
{
    public class CreateIntegration
    {
        public string TeamId { get; set; }

        public string SourceId { get; set; }

        public string Type { get; set; }

        public string MerchantId { get; set; }

        public string ReportUserName { get; set; }

        public string ReportPassword { get; set; }

        public string DefaultAccount { get; set; }
    }
}
