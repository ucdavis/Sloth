using Hangfire.Server;
using Serilog;
using Sloth.Jobs.Jobs.Attributes;
using Sloth.Jobs.Logging;

namespace Sloth.Jobs.Jobs
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
