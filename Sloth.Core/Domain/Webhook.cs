using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class WebHook
    {
        public string Id { get; set; }

        [Required]
        [JsonIgnore]
        public Team Team { get; set; }

        [DisplayName("Enabled")]
        public bool IsActive { get; set; }

        [DisplayName("Payload URL")]
        public string Url { get; set; }

        public string ContentType { get; set; }
    }
}
