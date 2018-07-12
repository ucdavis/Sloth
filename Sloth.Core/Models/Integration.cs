using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class Integration
    {
        public string Id { get; set; }

        [Required]
        public Team Team { get; set; }

        [Required]
        public Source Source { get; set; }

        [Required]
        public string Type { get; set; }

        public string MerchantId { get; set; }

        public string ReportUsername { get; set; }

        public string ReportPasswordKey { get; set; }

        public string DefaultAccount { get; set; }

        protected internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Integration>()
                .HasOne(i => i.Team)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Integration>()
                .HasOne(i => i.Source)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
