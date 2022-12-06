using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Core.Models
{
    public class TransactionMetadata
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionMetadata>()
                .HasIndex(r => r.TransactionId);

            // modelBuilder.Entity<TransactionMetadata>()
            //     .HasIndex(r => r.Name);
        }
    }
}
