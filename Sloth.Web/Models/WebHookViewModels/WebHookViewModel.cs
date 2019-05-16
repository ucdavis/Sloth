using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Web.Models.WebHookViewModels
{
    public class WebHookViewModel
    {
        [Required]
        [MaxLength(255)]
        [DisplayName("Payload URL")]
        public string Url { get; set; }

        public string ContentType = "application/json";

        [DisplayName("Enabled")]
        public bool IsActive { get; set; }
    }
}
