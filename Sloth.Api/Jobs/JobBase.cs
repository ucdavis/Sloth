using System;
using Hangfire.Server;
using Serilog;
using Sloth.Api.Jobs.Attributes;
using Sloth.Api.Logging;

namespace Sloth.Api.Jobs
{
    [ProlongExpirationTime]
    public class JobBase
    {
        protected ILogger Logger { get; set; }

        protected virtual void SetupLogging(PerformContext context)
        {
            Logger = LoggingConfiguration.Configuration
                .WriteTo.HangfireConsoleSink(context)
                .CreateLogger()
                .ForContext("jobId", context.BackgroundJob.Id);
        }
    }
}
