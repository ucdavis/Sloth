using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sloth.Api.Attributes;

namespace Sloth.Api.Models.v2
{
    public class CreateTransactionViewModel
    {
        public CreateTransactionViewModel()
        {
            AutoApprove = false;
            ValidateFinancialSegmentStrings = true;
            Transfers = new List<CreateTransferViewModel>();
        }

        /// <summary>
        /// Auto approve this transaction for upload to KFS
        /// Defaults to false
        /// </summary>
        public bool AutoApprove { get; set; }

        /// <summary>
        /// Run full validation on the financial segment strings
        /// Defaults to true
        /// </summary>
        public bool ValidateFinancialSegmentStrings { get; set; }

        /// <summary>
        /// Tracking Number created by the merchant accountant
        /// </summary>
        [MaxLength(128)]
        public string MerchantTrackingNumber { get; set; }

        /// <summary>
        ///  URL created by the merchant accountant referring to originating action
        /// </summary>
        public string MerchantTrackingUrl { get; set; }

        /// <summary>
        /// Tracking Number created by the payment processor
        /// </summary>
        [MaxLength(128)]
        public string ProcessorTrackingNumber { get; set; }

        /// <summary>
        /// Optionally set the kfs tracking number to be used
        /// </summary>
        [MaxLength(10)]
        public string KfsTrackingNumber { get; set; }

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

        /// <summary>
        /// Optional key/value pairs of generic metadata that will persist with this transaction
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        public string Description { get; set; }

        [ListMinLength(2)]
        [Required]
        public IList<CreateTransferViewModel> Transfers { get; set; }
    }
}
