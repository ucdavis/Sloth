using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class Source
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public string Description { get; set; }

        [MaxLength(2)]
        [Required]
        public string OriginCode { get; set; }

        [MaxLength(4)]
        [Required]
        public string DocumentType { get; set; }

        [JsonIgnore]
        [Required]
        public Team Team { get; set; }
    }
}
