using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class TransactionBlob
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string IntegrationId { get; set; }

        public Integration Integration { get; set; }

        public string TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        public string BlobId { get; set; }

        public Blob Blob { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionBlob>()
                .HasIndex(r => r.IntegrationId);

            modelBuilder.Entity<TransactionBlob>()
                .HasIndex(r => r.TransactionId);

            modelBuilder.Entity<TransactionBlob>()
                .HasIndex(r => r.BlobId);
        }

    }
}
