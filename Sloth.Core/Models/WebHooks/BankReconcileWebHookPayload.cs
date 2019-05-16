using System;

namespace Sloth.Core.Models.WebHooks
{
    public class BankReconcileWebHookPayload : WebHookPayload
    {
        public override string Action => "bank-reconcile";

        /// <summary>
        /// The tracking number that has been assigned to this deposit
        /// </summary>
        public string KfsTrackingNumber { get; set; }

        /// <summary>
        /// The tracking number assigned by the merchant
        /// </summary>
        public string MerchantTrackingNumber { get; set; }

        /// <summary>
        /// The tracking number assigned by the payment processor
        /// </summary>
        public string ProcessorTrackingNumber { get; set; }

        /// <summary>
        /// The date the transaction took place
        /// </summary>
        public DateTime TransactionDate { get; set; }
    }
}
