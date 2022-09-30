using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;
using Sloth.Web.Models.TransactionViewModels;

namespace Sloth.Web.Models.JobViewModels
{
    public class JobDetailsViewModel
    {
        public JobViewModel Job { get; set; }

        public TransactionsTableViewModel TransactionsTable { get; set; }
    }
}
