using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;

namespace Sloth.Api.Logging
{
    internal static class LoggingConfiguration
    {
        private static bool _loggingSetup;

        public static LoggerConfiguration Configuration => new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Stackify();

        /// <summary>
        /// Configure Application Logging
        /// </summary>
        public static void Setup(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (_loggingSetup) return; //only setup logging once

            // configure stackify
            var stackifyOptions = new StackifyOptions();
            configuration.GetSection("Stackify").Bind(stackifyOptions);
            StackifyLib.Logger.GlobalApiKey = stackifyOptions.ApiKey;
            StackifyLib.Logger.GlobalAppName = stackifyOptions.AppName;
            StackifyLib.Logger.GlobalEnvironment = stackifyOptions.Environment;

            // create global logger
            Log.Logger = Configuration
                .CreateLogger();

            _loggingSetup = true;
        }
    }

    public class StackifyOptions
    {
        public string AppName { get; set; }
        public string ApiKey { get; set; }
        public string Environment { get; set; }
    }
}
