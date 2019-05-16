using System;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Web.Models.IntegrationViewModels
{
    public class IntegrationViewModel
    {
        [Required]
        [Display(Name = "KFS Feed Source")]
        public string SourceId { get; set; }

        [Required]
        [Display(Name = "Integration Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Merchant Id")]
        public string MerchantId { get; set; }

        [Required]
        [Display(Name = "Report ApiKey Id")]
        public string ReportUserName { get; set; }

        [Required]
        [Display(Name = "Report ApiKey Secret")]
        public string ReportPassword { get; set; }

        [Required]
        [Display(Name = "Clearing Account")]
        public string ClearingAccount { get; set; }

        [Required]
        [Display(Name = "Holding Account")]
        public string HoldingAccount { get; set; }
    }
}
