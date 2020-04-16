using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;

namespace Sloth.Web.Models.TransactionViewModels
{
    public class TransactionsFilterViewModel
    {
        public  List<SelectListItem> TeamMerchantIds { get; set; }

        public TransactionsFilterModel Filter { get; set; }
    }
}
