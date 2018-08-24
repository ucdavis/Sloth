using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
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
        /// Chart of Accounts Code associated with Org Code
        /// </summary>
        [MaxLength(2)]
        [Required]
        public string Chart { get; set; }

        /// <summary>
        /// Financial System Organization responsible for GL Feed.
        /// </summary>
        [MaxLength(4)]
        [Required]
        [Display(Name = "Org Code")]
        public string OrganizationCode { get; set; }

        /// <summary>
        /// Unique feed origination identifier given to the Feed System.
        /// The origination code is validated in during file receipt and in the processing.
        /// </summary>
        [MinLength(2)]
        [MaxLength(2)]
        [Required]
        [Display(Name = "Origin Code")]
        public string OriginCode { get; set; }

        /// <summary>
        /// Financial System document type associated with the feed.
        /// Feed systems will be authorized to use a specific value based on transactions.
        /// </summary>
        [MinLength(4)]
        [MaxLength(4)]
        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        [JsonIgnore]
        [Display(Name = "KFS FTP Username")]
        public string KfsFtpUsername { get; set; }

        [JsonIgnore]
        [Display(Name = "KFS FTP Password Keyname")]
        public string KfsFtpPasswordKeyName { get; set; }

        [JsonIgnore]
        [Required]
        public Team Team { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Source>()
                .HasOne(i => i.Team)
                .WithMany(t => t.Sources)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
