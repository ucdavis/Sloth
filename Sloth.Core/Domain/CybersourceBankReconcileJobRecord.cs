using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class CybersourceBankReconcileJobRecord : JobRecord
    {
        [Display(Name = "Processed Date")]
        public DateTime ProcessedDate { get; set; }

        public IList<Transaction> Transactions { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CybersourceBankReconcileJobRecord>()
                .HasMany(r => r.Logs)
                .WithOne()
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CybersourceBankReconcileJobRecord>()
                .HasMany(r => r.Transactions)
                .WithOne(t => t.CybersourceReconcileJob)
                .HasForeignKey(t => t.CybersourceReconcileJobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
