using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Sloth.Core.Models;

namespace Sloth.Web.Models.TransactionViewModels
{
    public class TransactionsViewModel
    {
        public List<Transaction> Transactions { get; set; }

        public TransactionsFilterViewModel TransactionsFilter { get; set; }
    }
}
