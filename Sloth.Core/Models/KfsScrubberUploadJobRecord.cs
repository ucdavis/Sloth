using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class KfsScrubberUploadJobRecord
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Ran On")]
        public DateTime RanOn { get; set; }

        public string Status { get; set; }

        public IList<LogMessage> Logs { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KfsScrubberUploadJobRecord>()
                .HasMany(r => r.Logs)
                .WithOne()
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
