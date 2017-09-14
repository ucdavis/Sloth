using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sloth.Core.Models;

namespace Sloth.Api.Models
{
    public class CreateTransactionViewModel
    {
        public CreateTransactionViewModel()
        {
            Transfers = new List<Transfer>();
        }

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

        public IList<Transfer> Transfers { get; set; }
    }
}
