using System;
using Hangfire.Server;
using Serilog;

namespace Sloth.Api.Jobs
{
    [ProlongExpirationTime]
    public class JobBase
    {
        protected LoggerConfiguration LoggerConfiguration;

        public JobBase(LoggerConfiguration loggerConfiguration)
        {
            LoggerConfiguration = loggerConfiguration;
        }
    }
}
