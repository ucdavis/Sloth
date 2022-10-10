using System.Collections.Generic;
using Sloth.Core.Models;
using Sloth.Web.Models.BlobViewModels;

namespace Sloth.Web.Models.TransactionViewModels
{
    public class TransactionDetailsViewModel
    {
        public TransactionDetailsViewModel()
        {
            RelatedTransactions = new TransactionsTableViewModel();
            RelatedTransactions.Transactions = new List<Transaction>();
            RelatedBlobs = new BlobsTableViewModel();
        }

        public Transaction Transaction { get; set; }
        public TransactionsTableViewModel RelatedTransactions { get; set; }
        public BlobsTableViewModel RelatedBlobs { get; set; }

        public bool HasWebhooks { get; set; } = false;
    }
}
