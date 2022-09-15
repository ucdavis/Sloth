using System;

namespace Sloth.Web.Models.JobViewModels
{
    public class JobViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public string Status { get; set; }

        public object Details { get; set; }
    }
}
