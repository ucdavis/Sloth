using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class WebHookRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string WebHookId { get; set; }

        public WebHook WebHook { get; set; }

        public string Payload { get; set; }

        [DisplayName("Response Status")]
        public int? ResponseStatus { get; set; }

        [DisplayName("Response Body")]
        public string ResponseBody { get; set; }

        [DisplayName("Request Count")]
        public int? RequestCount { get; set; }

        [DisplayName("Last Request Date")]
        public DateTime? LastRequestDate { get; set; }

        public bool Persist { get; set; }

        public WebHookRequestResendJobRecord WebHookRequestResendJob { get; set; }

        public string WebHookRequestResendJobId { get; set; }
    }
}
