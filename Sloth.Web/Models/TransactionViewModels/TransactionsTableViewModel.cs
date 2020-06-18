using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;

namespace Sloth.Web.Models.TransactionViewModels
{
    public class TransactionsTableViewModel
    {
        public IList<Transaction> Transactions { get; set; }

        public bool HasWebhooks { get; set; }
    }
}
