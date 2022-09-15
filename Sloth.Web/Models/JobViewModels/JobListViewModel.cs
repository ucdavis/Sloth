using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;

namespace Sloth.Web.Models.JobViewModels
{
    public class JobListViewModel
    {

        public string JobName { get; set; }

        public List<JobViewModel> Jobs { get; set; }

        public JobsFilterModel Filter { get; set; }
    }
}
