using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class Integration
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [JsonIgnore]
        public Team Team { get; set; }

        [Required]
        [JsonIgnore]
        public Source Source { get; set; }

        [Required]
        public string Type { get; set; }

        public string MerchantId { get; set; }

        [JsonIgnore]
        public string ReportUsername { get; set; }

        [JsonIgnore]
        public string ReportPasswordKey { get; set; }

        [Required]
        [MaxLength(128)]
        public string ClearingAccount { get; set; }

        [Required]
        [MaxLength(128)]
        public string HoldingAccount { get; set; }

        public IList<TransactionBlob> JobRecordBlobs { get; set; }

        protected internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Integration>()
                .HasOne(i => i.Team)
                .WithMany(t => t.Integrations)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Integration>()
                .HasOne(i => i.Source)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Integration>()
                .HasMany(i => i.JobRecordBlobs)
                .WithOne(b => b.Integration)
                .HasForeignKey(b => b.IntegrationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
