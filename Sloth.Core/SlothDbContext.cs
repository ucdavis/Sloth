using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sloth.Core.Domain;
using Sloth.Core.Models;

namespace Sloth.Core
{
    public class SlothDbContext : IdentityDbContext<User>
    {
        public SlothDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<ApiKey> ApiKeys { get; set; }

        public virtual DbSet<Integration> Integrations { get; set; }

        public virtual DbSet<Scrubber> Scrubbers { get; set; }

        public virtual DbSet<Source> Sources { get; set; }

        public virtual DbSet<JobRecord> JobRecords { get; set; }

        public virtual DbSet<JournalRequest> JournalRequests { get; set; }

        public virtual DbSet<Transaction> Transactions { get; set; }

        public virtual DbSet<Transfer> Transfers { get; set; }

        public virtual DbSet<Team> Teams { get; set; }

        public virtual DbSet<TeamRole> TeamRoles { get; set; }

        public virtual DbSet<UserTeamRole> UserTeamRoles { get; set; }

        public virtual DbSet<CybersourceBankReconcileJobRecord> CybersourceBankReconcileJobRecords { get; set; }

        public virtual DbSet<KfsScrubberUploadJobRecord> KfsScrubberUploadJobRecords { get; set; }

        public virtual DbSet<LogMessage> Logs { get; set; }

        public virtual DbSet<WebHook> WebHooks { get; set; }

        public virtual DbSet<Blob> Blobs { get; set; }

        public virtual DbSet<CybersourceBankReconcileJobBlob> CybersourceBankReconcileJobBlobs { get; set; }

        public virtual DbSet<TransactionStatusEvent> TransactionStatusEvents { get; set; }

        public virtual DbSet<WebHookRequest> WebHookRequests { get; set; }

        public virtual DbSet<WebHookRequestResendJobRecord> WebHookRequestResendJobRecords { get; set; }

        public virtual async Task<string> GetNextKfsTrackingNumber(DbTransaction transaction = null)
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

        public virtual async Task<string> GetNextDocumentNumber(DbTransaction transaction = null)
        {
            const string sql = "SELECT NEXT VALUE FOR Document_Number_Seq AS DocumentNumber";

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
                    return reader.GetInt64(0).ToString("D9");
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
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasSequence("Document_Number_Seq")
                .StartsAt(1)
                .HasMin(1)
                .HasMax(999_999_999)
                .IncrementsBy(1)
                .IsCyclic();

            modelBuilder.HasSequence("KFS_Tracking_Number_Seq")
                .StartsAt(1)
                .HasMin(1)
                .HasMax(9_999_999_999)
                .IncrementsBy(1)
                .IsCyclic();

            ApiKey.OnModelCreating(modelBuilder);
            User.OnModelCreating(modelBuilder);
            Team.OnModelCreating(modelBuilder);
            TeamRole.OnModelCreating(modelBuilder);
            UserTeamRole.OnModelCreating(modelBuilder);
            Source.OnModelCreating(modelBuilder);
            Integration.OnModelCreating(modelBuilder);
            Transaction.OnModelCreating(modelBuilder);
            LogMessage.OnModelCreating(modelBuilder);
            CybersourceBankReconcileJobRecord.OnModelCreating(modelBuilder);
            KfsScrubberUploadJobRecord.OnModelCreating(modelBuilder);
            Blob.OnModelCreating(modelBuilder);
            TransactionStatusEvent.OnModelCreating(modelBuilder);
            WebHook.OnModelCreating(modelBuilder);
            WebHookRequest.OnModelCreating(modelBuilder);
            WebHookRequestResendJobRecord.OnModelCreating(modelBuilder);
        }
    }
}
