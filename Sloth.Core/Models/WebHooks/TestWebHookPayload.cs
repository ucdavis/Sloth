using System;

namespace Sloth.Core.Models.WebHooks
{
    public class TestWebHookPayload : WebHookPayload
    {
        public override string Action => "test";

        public string HookId { get; set; }
    }
}
