using Hangfire.RecurringJobExtensions;
using Hangfire.Server;

namespace Sloth.Jobs.Jobs
{
    public class Heartbeat : JobBase
    {
        public Heartbeat() : base("Heartbeat")
        {
        }

        [RecurringJob(CronStrings.Minutely, RecurringJobId = "heartbeat")]
        public void Fire(PerformContext context)
        {
            SetupLogging(context);

            Logger.Information("Tick");
        }
    }
}
