using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Core.Jobs
{
    /// <summary>
    /// Just a wrapper around <see cref="IWebHookService"/> to be consistent with other jobs
    /// </summary>
    public class ResendPendingWebHookRequestsJob
    {
        public const string JobName = "WebHooks.Resend";
        private readonly IWebHookService _webHookService;

        public ResendPendingWebHookRequestsJob(IWebHookService webHookService)
        {
            _webHookService = webHookService;
        }

        public async Task<WebHookRequestJobDetails> ResendPendingWebHookRequests()
        {
            var jobDetails = new WebHookRequestJobDetails();
            var requests = await _webHookService.ResendPendingWebHookRequests();
            jobDetails.WebHookRequestIds = requests.Select(r => r.Id).ToList();
            return jobDetails;
        }

        public class WebHookRequestJobDetails
        {
            public List<string> WebHookRequestIds { get; set; } = new ();
        }
    }
}
