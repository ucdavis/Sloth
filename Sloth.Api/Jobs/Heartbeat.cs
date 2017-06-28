using Hangfire.RecurringJobExtensions;
using Hangfire.Server;

namespace Sloth.Api.Jobs
{
    public class Heartbeat : JobBase
    {
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
