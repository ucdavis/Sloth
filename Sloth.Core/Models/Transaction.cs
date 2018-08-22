using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Sloth.Core.Models
{
    public class Transaction
    {
        public Transaction()
        {
            Transfers = new List<Transfer>();
        }

        public string Id { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public User Creator { get; set; }

        [NotMapped]
        public string CreatorName => Creator?.UserName;

        public string Status { get; set; }

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
        [DisplayName("Merchant Tracking Number")]
        public string MerchantTrackingNumber { get; set; }

        /// <summary>
        /// URL created by the merchant to track back
        /// </summary>
        [Display(Name="Merchant Url")]
        public string MerchantTrackingUrl { get; set; }

        /// <summary>
        /// Tracking Number created by the payment processor
        /// </summary>
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

        public void AddReversalTransaction(Transaction transaction)
        {
            // setup one to one
            this.ReversalTransaction = transaction;
            transaction.ReversalOfTransaction = this;
        }
    }
}
