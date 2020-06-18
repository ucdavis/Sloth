using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sloth.Core.Models;
using Sloth.Web.Models.TransactionViewModels;

namespace Sloth.Web.Models.ScrubberViewModels
{
    public class ScrubberDetailsViewModel
    {
        public Scrubber Scrubber { get; set; }

        public TransactionsTableViewModel TransactionsTable { get; set; }
    }
}
