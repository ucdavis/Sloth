using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Sloth.Core.Models;

namespace Sloth.Web.Models.TransactionViewModels
{
    public class TransactionsReturnedViewModel
    {
        public string SelectedMerchantId { get; set; }

        public  List<string> TeamMerchantIds { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime From { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime To { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}
