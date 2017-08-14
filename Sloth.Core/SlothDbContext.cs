using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sloth.Core.Models;

namespace Sloth.Core
{
    public class SlothDbContext : DbContext
    {
        public SlothDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ApiKey> ApiKeys { get; set; }

        public DbSet<Integration> Integrations { get; set; }

        public DbSet<Scrubber> Scrubbers { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Transfer> Transfers { get; set; }

        public DbSet<User> Users { get; set; }

        public async Task<string> GetNextKfsTrackingNumber(DbTransaction transaction = null)
        {
            const string sql = "SELECT NEXT VALUE FOR KFS_Tracking_Number_Seq AS KfsTrackingNumber";

            var conn = Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                if (transaction != null)
                {
                    cmd.Transaction = transaction;
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    return reader.GetInt64(0).ToString("D10");
                }
            }
        }

        public IEnumerable<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(model, context, results, true)) yield break;

            foreach (var validationResult in results.Where(r => r != ValidationResult.Success).ToList())
                yield return validationResult;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence("KFS_Tracking_Number_Seq")
                .StartsAt(1)
                .HasMin(1)
                .HasMax(9_999_999_999)
                .IncrementsBy(1)
                .IsCyclic();
        }
    }
}