using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;

namespace Sloth.Web.Models.TransactionViewModels
{
    public class TransactionsIndexViewModel
    {
        public List<SelectListItem> TeamMerchantIds { get; set; }

        public TransactionsFilterModel Filter { get; set; }

        public TransactionsTableViewModel TransactionsTable { get; set; }
    }
}
