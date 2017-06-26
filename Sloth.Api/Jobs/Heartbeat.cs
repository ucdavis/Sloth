using System;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Serilog;
using Serilog.Core;

namespace Sloth.Api.Jobs
{
    public class Heartbeat : JobBase
    {
        private Logger Logger { get; set; }

        public Heartbeat(LoggerConfiguration loggerConfiguration) : base(loggerConfiguration)
        {
        }

        [RecurringJob(CronStrings.Minutely, RecurringJobId = "heartbeat")]
        public void Fire(PerformContext context)
        {
            Logger = LoggerConfiguration
                .WriteTo.HangfireConsoleSink(context)
                .CreateLogger();

            Logger
                .ForContext("jobId", context.BackgroundJob.Id)
                .Information("Tick");
        }
    }
}
