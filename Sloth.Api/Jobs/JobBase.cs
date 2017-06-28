using System;
using Hangfire.Server;
using Serilog;
using Serilog.Core;

namespace Sloth.Api.Jobs
{
    [ProlongExpirationTime]
    public class JobBase
    {
        protected Logger Logger { get; set; }

        private readonly LoggerConfiguration _loggerConfiguration;

        public JobBase(LoggerConfiguration loggerConfiguration)
        {
            _loggerConfiguration = loggerConfiguration;
        }

        protected void SetupLogging(PerformContext context)
        {
            Logger = _loggerConfiguration
                .WriteTo.HangfireConsoleSink(context)
                .CreateLogger();
        }
    }
}
