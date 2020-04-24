using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;

namespace Sloth.Web.Models.JobViewModels
{
    public class KfsScrubberJobsViewModel
    {
        public List<KfsScrubberUploadJobRecord> Jobs { get; set; }
        
        public JobsFilterModel Filter { get; set; }
    }
}
