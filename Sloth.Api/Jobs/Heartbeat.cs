using System;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Serilog;

namespace Sloth.Api.Jobs
{
    public class Heartbeat : JobBase
    {
        public Heartbeat(LoggerConfiguration loggerConfiguration) : base(loggerConfiguration)
        {
        }

        [RecurringJob(CronStrings.Minutely, RecurringJobId = "heartbeat")]
        public void Fire(PerformContext context)
        {
            SetupLogging(context);

            Logger
                .ForContext("jobId", context.BackgroundJob.Id)
                .Information("Tick");
        }
    }
}
