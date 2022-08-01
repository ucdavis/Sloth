using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Sloth.Core.Domain;

namespace Sloth.Core.Models
{
    public class CybersourceBankReconcileJobRecord : JobRecordBase
    {
        [Display(Name = "Processed Date")]
        public DateTime ProcessedDate { get; set; }

        public IList<Transaction> Transactions { get; set; }

        public IList<CybersourceBankReconcileJobBlob> CybersourceBankReconcileJobBlobs { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CybersourceBankReconcileJobRecord>()
                .HasMany(r => r.Logs)
                .WithOne()
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CybersourceBankReconcileJobRecord>()
                .HasMany(r => r.Transactions)
                .WithOne(t => t.CybersourceBankReconcileJob)
                .HasForeignKey(t => t.CybersourceBankReconcileJobRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CybersourceBankReconcileJobRecord>()
                .HasMany(blob => blob.CybersourceBankReconcileJobBlobs)
                .WithOne(r => r.CybersourceBankReconcileJobRecord)
                .HasForeignKey(r => r.CybersourceBankReconcileJobRecordId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
