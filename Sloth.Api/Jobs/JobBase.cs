using System;
using Hangfire.Server;
using Serilog.Core;
using Sloth.Api.Jobs.Attributes;
using Sloth.Api.Logging;

namespace Sloth.Api.Jobs
{
    [ProlongExpirationTime]
    public class JobBase
    {
        protected Logger Logger { get; set; }

        protected void SetupLogging(PerformContext context)
        {
            Logger = LoggingConfiguration.Configuration
                .WriteTo.HangfireConsoleSink(context)
                .CreateLogger();
        }
    }
}
