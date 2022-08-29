using System.Collections.Generic;
using Sloth.Core.Models;

namespace Sloth.Web.Models.TransactionViewModels
{
    public class TransactionDetailsViewModel
    {
        public TransactionDetailsViewModel()
        {
            RelatedTransactions = new TransactionsTableViewModel();
            RelatedTransactions.Transactions = new List<Transaction>();
            //RelatedTransactions.HasWebhooks = false;
        }

        public Transaction Transaction { get; set; }
        public TransactionsTableViewModel RelatedTransactions { get; set; }

        public bool HasWebhooks { get; set; } = false;
    }
}
