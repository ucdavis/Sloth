using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class Source
    {
        public string Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string Type { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        /// <summary>
        /// Unique feed origination identifier given to the Feed System.
        /// The origination code is validated in during file receipt and in the processing.
        /// </summary>
        [MinLength(2)]
        [MaxLength(2)]
        [Required]
        public string OriginCode { get; set; }

        /// <summary>
        /// Financial System document type associated with the feed.
        /// Feed systems will be authorized to use a specific value based on transactions.
        /// </summary>
        [MinLength(4)]
        [MaxLength(4)]
        [Required]
        public string DocumentType { get; set; }

        [JsonIgnore]
        [Required]
        public Team Team { get; set; }
    }
}
