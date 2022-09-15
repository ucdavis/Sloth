using System;
using Sloth.Core.Jobs;
using Sloth.Web.Controllers;

namespace Sloth.Web.Models.JobViewModels
{
    public static class JobViewModelExtensions
    {
        public static string GetDetailsUrl(this JobViewModel job)
        {
            switch (job.Name)
            {
                case  CybersourceBankReconcileJob.JobName:
                    return $"/jobs/{nameof(JobsController.CybersourceBankReconcileDetails)}/{job.Id}";
                case  KfsScrubberUploadJob.JobName:
                    return $"/jobs/{nameof(JobsController.KfsScrubberUploadDetails)}/{job.Id}";
                case  ResendPendingWebHookRequestsJob.JobName:
                    return $"/jobs/{nameof(JobsController.WebhookResendDetails)}/{job.Id}";
                default:
                    return $"/jobs/{nameof(JobsController.JobDetails)}/{job.Id}";
            }
        }
    }
}
