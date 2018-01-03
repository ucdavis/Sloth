using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sloth.Api.Attributes;

namespace Sloth.Api.Models
{
    public class CreateTransactionViewModel
    {
        public CreateTransactionViewModel()
        {
            AutoApprove = false;
            Transfers = new List<CreateTransferViewModel>();
        }

        /// <summary>
        /// Auto approve this transaction for upload to KFS
        /// </summary>
        public bool AutoApprove { get; set; }

        /// <summary>
        /// Tracking Number created by the merchant accountant
        /// </summary>
        public string MerchantTrackingNumber { get; set; }

        /// <summary>
        /// Tracking Number created by the payment processor
        /// </summary>
        public string ProcessorTrackingNumber { get; set; }

        /// <summary>
        /// Date the transaction occurred.
        /// </summary>
        [Required]
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Source of the transactions
        /// e.g. CyberSource
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        /// Type of the transactions by their source
        /// e.g. Income
        /// </summary>
        [Required]
        public string SourceType { get; set; }

        [ListMinLength(2)]
        [Required]
        public IList<CreateTransferViewModel> Transfers { get; set; }
    }
}
