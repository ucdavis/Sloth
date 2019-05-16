using System;

namespace Sloth.Core.Models.WebHooks
{
    public abstract class WebHookPayload
    {
        public abstract string Action { get; }
    }
}
