using System;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class TransactionMetadata
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        public string Name { get; set; }

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
