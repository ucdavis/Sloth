using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Sloth.Core.Models;

namespace Sloth.Web.Models.JobViewModels
{
    public class CybersourceBankReconcileJobsFilterModel
    {
        public DateTime? SearchDate { get; set; }

        public string SearchType { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? Date { get; set; }
    }
}
