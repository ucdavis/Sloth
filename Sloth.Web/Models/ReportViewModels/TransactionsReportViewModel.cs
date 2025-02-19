using System;
using Sloth.Web.Models.TransactionViewModels;

namespace Sloth.Web.Models.ReportViewModels
{
    public class TransactionsReportViewModel
    {
        public TransactionsTableViewModel TransactionsTable { get; set; }

        public string? Information { get; set; }
    }
}
