using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class Transaction
    {
        public Transaction()
        {
            Transfers = new List<Transfer>();
            StatusEvents = new List<TransactionStatusEvent>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Description { get; set; }

        [JsonIgnore]
        public User Creator { get; set; }

        [NotMapped]
        public string CreatorName => Creator?.UserName;

        // Status updates must go through SetStatus to ensure StatusEvents are properly updated
        [MaxLength(20)]
        public string Status { get; private set; }

        [JsonIgnore]
        [Required]
        public Source Source { get; set; }

        [NotMapped]
        public string SourceName => Source?.Name;

        [NotMapped]
        public string SourceType => Source?.Type;


        /// <summary>
        /// Tracking Number created by the merchant accountant
        /// </summary>
        [MaxLength(128)]
        [DisplayName("Merchant Tracking Number")]
        public string MerchantTrackingNumber { get; set; }

        /// <summary>
        /// URL created by the merchant to track back
        /// </summary>
        [Display(Name = "Merchant Url")]
        public string MerchantTrackingUrl { get; set; }

        /// <summary>
        /// Tracking Number created by the payment processor
        /// </summary>
        [MaxLength(128)]
        [DisplayName("Processor Tracking Number")]
        public string ProcessorTrackingNumber { get; set; }

        /// <summary>
        /// Unique feed origination identifier given to the Feed System.
        /// The origination code is validated in during file receipt and in the processing.
        /// </summary>
        [NotMapped]
        public string OriginCode => Source?.OriginCode;

        /// <summary>
        /// Unique identifier for a set of related transactions per origination code.
        /// A file can have multiple document numbers but the file must balance by document number(aka net zero) and by total amount.Debits = Credits
        /// Once a document number posts to the general ledger then it cannot be used again.
        /// </summary>
        [MinLength(1)]
        [MaxLength(14)]
        [RegularExpression("[A-Z0-9]*")]
        [DisplayName("Document Number")]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Financial System document type associated with the feed.
        /// Feed systems will be authorized to use a specific value based on transactions.
        /// </summary>
        [NotMapped]
        public string DocumentType => Source?.DocumentType;

        /// <summary>
        /// Primarily used in Decision Support reporting for additional transaction identification.
        /// Equivalent to the KFS Organization Document Number.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [DisplayName("Kfs Tracking Number")]
        public string KfsTrackingNumber { get; set; }

        /// <summary>
        /// Date the transaction occurred.
        /// </summary>
        [Required]
        [DisplayName("Transaction Date")]
        public DateTime TransactionDate { get; set; }

        public IList<Transfer> Transfers { get; set; }

        [JsonIgnore]
        public Scrubber Scrubber { get; set; }

        public string ScrubberId { get; set; }

        [JsonIgnore]
        public JournalRequest JournalRequest { get; set; }

        [JsonIgnore]
        [DisplayName("Reversal for Transaction")]
        public Transaction ReversalOfTransaction { get; set; }

        [DisplayName("Reversal for Transaction Id")]
        public string ReversalOfTransactionId { get; set; }

        [NotMapped]
        [DisplayName("Is Reversal Transaction")]
        public bool IsReversal => !string.IsNullOrEmpty(ReversalOfTransactionId);

        [JsonIgnore]
        [DisplayName("Reversal Transaction")]
        public Transaction ReversalTransaction { get; set; }

        [DisplayName("Reversal Transaction Id")]
        public string ReversalTransactionId { get; set; }

        [NotMapped]
        [DisplayName("Has Reversal Transaction")]
        public bool HasReversal => !string.IsNullOrEmpty(ReversalTransactionId);

        public IList<TransactionStatusEvent> StatusEvents { get; set; }

        public IList<TransactionBlob> TransactionBlobs { get; set; }

        public void AddReversalTransaction(Transaction transaction)
        {
            // setup bidirectional relationship
            this.ReversalTransaction = transaction;
            transaction.ReversalOfTransaction = this;
        }

        public Transaction SetStatus(string status, string details = "", [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            StatusEvents.Add(new TransactionStatusEvent
            {
                TransactionId = Id,
                Status = status,
                EventDate = DateTime.UtcNow,
                EventDetails = !string.IsNullOrWhiteSpace(details)
                    ? details
                    : $"File: {Path.GetFileName(sourceFilePath)}, Member: {memberName}, Line: {sourceLineNumber}. {details}",
            });

            Status = status;

            return this;
        }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ReversalTransaction)
                .WithOne()
                .HasForeignKey<Transaction>(t => t.ReversalTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ReversalOfTransaction)
                .WithOne()
                .HasForeignKey<Transaction>(t => t.ReversalOfTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasMany(t => t.StatusEvents)
                .WithOne(e => e.Transaction)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasMany(t => t.TransactionBlobs)
                .WithOne(b => b.Transaction)
                .HasForeignKey(r => r.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.KfsTrackingNumber);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.MerchantTrackingNumber);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.ProcessorTrackingNumber);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Status);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionDate);
        }
    }
}
