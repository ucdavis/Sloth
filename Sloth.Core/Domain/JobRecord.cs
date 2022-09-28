using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class JobRecord
    {
        public JobRecord()
        {
            StartedAt = DateTime.UtcNow;
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public int? TotalTransactions { get; set; }

        [StringLength(32)]
        public string Status { get; set; }

        public string Details { get; set; }

        public void SetDetails(object details)
        {
            // convert to json string and store
            Details = JsonSerializer.Serialize(details);
        }

        public void SetCompleted(string status, object details)
        {
            EndedAt = DateTime.UtcNow;
            Status = status;
            SetDetails(details);
        }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobRecord>()
                .HasIndex(j => j.Name);

            modelBuilder.Entity<JobRecord>()
                .HasIndex(j => j.StartedAt);

            modelBuilder.Entity<JobRecord>()
                .HasIndex(j => j.EndedAt);

            modelBuilder.Entity<JobRecord>()
                .HasIndex(j => j.ProcessedDate);

            modelBuilder.Entity<JobRecord>()
                .HasIndex(j => j.Status);
        }

        // list of possible statuses
        public static class Statuses
        {
            public const string Pending = "Pending";
            public const string Running = "Running";
            public const string Success = "Success";
            public const string Failed = "Failed";
        }
    }
}
