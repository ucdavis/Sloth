using System;
using System.ComponentModel.DataAnnotations;
using Sloth.Core.Models;

namespace Sloth.Api.Models.v2
{
    public class CreateTransferViewModel
    {
        /// <summary>
        /// Dollar amount associated with the transaction
        /// </summary>
        [Range(typeof(decimal), "0.01", "1000000000")]
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Aggie Enterprise: Full COA string (GL or PPM)
        /// To use Aggie Enterprise, fill out this field instead of Chat, Account, etc.
        /// </summary>
        [Required]
        public string FinancialSegmentString { get; set; }

        /// <summary>
        /// A brief description of the specific transaction. Displays in reporting.
        /// PCI, HIPPA, FERPA and PII information is prohibited.
        /// </summary>
        [MaxLength(40)]
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Optional: The accounting date of the transfer. Will be defaulted to today's date if not provided.
        /// </summary>
        public DateTime? AccountingDate { get; set; }

        /// <summary>
        /// Debit or Credit Code associated with the transaction.
        /// </summary>
        [Required]
        public Transfer.CreditDebit Direction { get; set; }

        /// <summary>
        /// Optional field for organizational reference purposes.
        /// </summary>
        [MinLength(1)]
        [MaxLength(8)]
        public string ReferenceId { get; set; }
    }
}
