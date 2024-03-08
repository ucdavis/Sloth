using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Sloth.Core.Models;
using System.Collections.Generic;
using Sloth.Web.Models.TransactionViewModels;
using System.Linq.Expressions;
using System.Linq;

namespace Sloth.Web.Models.ReportViewModels
{
    public class TransfersReportViewModel
    {
        public TransactionsFilterModel Filter { get; set; }

        public IList<TransactionWithTransfers> Transactions { get; set; }
    }

    public class TransactionWithTransfers
    {
        public string Id { get; set; }
        public string DisplayId => $"...{Id[^4..]}"; // last 4 characters of the id

        public string Status { get; set; }

        [DisplayName("Transaction Date")]
        public DateTime TransactionDate { get; set; }

        [DisplayName("Kfs Tracking Number")]
        public string KfsTrackingNumber { get; set; }
        [DisplayName("Document Number")]
        public string DocumentNumber { get; set; }

        [DisplayName("Processor Tracking Number")]
        public string ProcessorTrackingNumber { get; set; }

        [DisplayName("Merchant Tracking Number")]
        public string MerchantTrackingNumber { get; set; }
        [DisplayName("Transaction Description")]
        public string TxnDescription { get; set; }

        public string MetaDataString { get; set; }

        [DisplayName("Transaction Amount")]
        public decimal Amount { get; set; }

        public int TransferCount { get; set; }

        public IList<Transfer> Transfers { get; set; } // don't need all the info in here, but it shouldn't be too big.

        public static Expression<Func<Transaction, TransactionWithTransfers>> Projection()
        {

            return txn => new TransactionWithTransfers
            {
                Id = txn.Id,
                Status = txn.Status,
                TransactionDate = txn.TransactionDate,
                KfsTrackingNumber = txn.KfsTrackingNumber,
                DocumentNumber = txn.DocumentNumber,
                ProcessorTrackingNumber = txn.ProcessorTrackingNumber,
                MerchantTrackingNumber = txn.MerchantTrackingNumber,
                TxnDescription = txn.Description,
                MetaDataString = string.Join(", ", txn.Metadata.Select(kv => $"{kv.Name}: {kv.Value}")),
                Amount = txn.Transfers.Where(a => a.Direction == Transfer.CreditDebit.Credit).Sum(a => a.Amount),
                TransferCount = txn.Transfers.Count,
                Transfers = txn.Transfers,
            };

        }
    }
}
