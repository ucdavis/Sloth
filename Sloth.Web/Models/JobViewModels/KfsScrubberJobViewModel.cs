using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;
using Sloth.Web.Models.TransactionViewModels;

namespace Sloth.Web.Models.JobViewModels
{
    public class KfsScrubberJobViewModel
    {
        public KfsScrubberUploadJobRecord Job { get; set; }
        
        public TransactionsTableViewModel TransactionsTable { get; set; }

        [Display(Name = "Transaction Count")]
        public int TransactionCount { get; set; }
    }
}
