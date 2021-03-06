using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sloth.Core.Domain;

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

        public string ClearingAccount { get; set; }

        public string HoldingAccount { get; set; }

        public IList<CybersourceBankReconcileJobBlob> CybersourceBankReconcileJobBlobs { get; set; }


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
                .HasMany(i => i.CybersourceBankReconcileJobBlobs)
                .WithOne(b => b.Integration)
                .HasForeignKey(b => b.IntegrationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
