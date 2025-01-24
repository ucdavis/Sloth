using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Sloth.Web.Models.SourceViewModels
{
    public class SourceViewModel
    {
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        [MaxLength(50)]
        [Required]
        [Display(Name = "Source Type")]
        public string Type { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        
    }
}
