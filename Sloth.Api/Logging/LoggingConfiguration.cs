using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;

namespace Sloth.Api.Logging
{
    internal static class LoggingConfiguration
    {
        private static bool _loggingSetup;

        public static LoggerConfiguration Configuration { get; set; }

        /// <summary>
        /// Configure Application Logging
        /// </summary>
        public static void Setup(IHostingEnvironment env, IConfiguration configuration)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (_loggingSetup) return; //only setup logging once

            // configure stackify
            var stackifyOptions = new StackifyOptions();
            configuration.GetSection("Stackify").Bind(stackifyOptions);
            StackifyLib.Logger.GlobalApiKey = stackifyOptions.ApiKey;
            StackifyLib.Logger.GlobalAppName = stackifyOptions.AppName;
            StackifyLib.Logger.GlobalEnvironment = stackifyOptions.Environment;

            // configure serilog
            Configuration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Stackify();

            if (env.IsDevelopment())
            {
                Configuration = Configuration
                    .WriteTo.LiterateConsole()
                    .WriteTo.Trace();
            }

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
