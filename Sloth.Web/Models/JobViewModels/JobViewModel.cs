using System;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Web.Models.JobViewModels
{
    public class JobViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Started At")]
        public DateTime StartedAt { get; set; }

        [Display(Name = "Ended At")]
        public DateTime? EndedAt { get; set; }

        public string Status { get; set; }

        [Display(Name = "Transaction Count")]
        public int TransactionCount { get; set; }

        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDate { get; set; }

        public object Details { get; set; }
    }
}
