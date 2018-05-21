using Hangfire.Server;
using Serilog;
using Sloth.Jobs.Jobs.Attributes;
using Sloth.Jobs.Logging;

namespace Sloth.Jobs.Jobs
{
    [ProlongExpirationTime]
    public class JobBase
    {
        private readonly string _jobName;

        public JobBase(string jobName)
        {
            _jobName = jobName;
        }

        protected ILogger Logger { get; set; }

        protected virtual void SetupLogging(PerformContext context)
        {
            Logger = LoggingConfiguration.GetAuditConfiguration()
                .WriteTo.HangfireConsoleSink(context)
                .CreateLogger()
                .ForContext("jobId", context.BackgroundJob.Id)
                .ForContext("jobName", _jobName);
        }
    }
}
