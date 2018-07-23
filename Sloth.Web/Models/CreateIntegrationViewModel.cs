using System.ComponentModel.DataAnnotations;

namespace Sloth.Web.Models
{
    public class CreateIntegrationViewModel
    {
        [Required]
        [Display(Name = "Team")]
        public string TeamId { get; set; }

        [Required]
        [Display(Name = "Source")]
        public string SourceId { get; set; }

        [Required]
        [Display(Name = "Integration Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Merchant Id")]
        public string MerchantId { get; set; }

        [Required]
        [Display(Name = "Report Username")]
        public string ReportUserName { get; set; }

        [Required]
        [Display(Name = "Report Password")]
        public string ReportPassword { get; set; }

        [Required]
        [Display(Name = "Default Account")]
        public string DefaultAccount { get; set; }
    }
}
