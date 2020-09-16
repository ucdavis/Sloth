using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class KfsScrubberUploadJobRecord : JobRecord
    {
        public IList<Transaction> Transactions { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KfsScrubberUploadJobRecord>()
                .HasMany(r => r.Logs)
                .WithOne()
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KfsScrubberUploadJobRecord>()
                .HasMany(r => r.Transactions)
                .WithOne(t => t.KfsScrubberUploadJob)
                .HasForeignKey(t => t.KfsScrubberUploadJobRecordId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
