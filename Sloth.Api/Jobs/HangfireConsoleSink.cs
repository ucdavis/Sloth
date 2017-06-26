using System;
using Hangfire.Console;
using Hangfire.Server;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Sloth.Api.Jobs
{
    internal class HangfireConsoleSink : ILogEventSink
    {
        private readonly PerformContext _context;

        public HangfireConsoleSink(PerformContext context)
        {
            _context = context;
        }

        public void Emit(LogEvent logEvent)
        {
            _context.WriteLine(logEvent.RenderMessage());
        }
    }

    internal static class HangfireConsoleSinkExtensions
    {
        public static LoggerConfiguration HangfireConsoleSink(
            this LoggerSinkConfiguration loggerConfiguration,
            PerformContext context,
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));

            return loggerConfiguration
                .Sink(new HangfireConsoleSink(context), restrictedToMinimumLevel);
        }
    }
}
