using System;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class LogMessage
    {
        public int Id { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public string MessageTemplate { get; set; }

        public string Level { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string Exception { get; set; }

        public string Properties { get; set; }

        public string LogEvent { get; set; }

        public string CorrelationId { get; set; }

        public string JobId { get; set; }

        public string JobName { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogMessage>()
                .ToTable("Logs");

            modelBuilder.Entity<LogMessage>()
                .HasIndex(l => l.Source);

            modelBuilder.Entity<LogMessage>()
                .HasIndex(l => l.CorrelationId);

            modelBuilder.Entity<LogMessage>()
                .HasIndex(l => new { l.JobName, l.JobId });

            modelBuilder.Entity<LogMessage>()
                .Property(l => l.Properties)
                .HasColumnType("xml");
        }
    }
}
