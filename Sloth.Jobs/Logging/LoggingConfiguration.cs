using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.MSSqlServer;

namespace Sloth.Jobs.Logging
{
    public static class LoggingConfiguration
    {
        private static bool _loggingSetup;

        public static LoggerConfiguration Configuration;

        /// <summary>
        /// Configure Application Logging
        /// </summary>
        public static void Setup(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (_loggingSetup) return; //only setup logging once

            if (true)
            {
                Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            }

            // standard logger
            Configuration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Source", "Sloth.Jobs");

            // various sinks
            ConfigureSqlLogging(configuration);

            ConfigureStackifyLogging(configuration);

            // create global logger
            Log.Logger = Configuration.CreateLogger();

            _loggingSetup = true;
        }

        private static void ConfigureStackifyLogging(IConfiguration configuration)
        {
            var stackifyOptions = new StackifyOptions();
            configuration.GetSection("Stackify").Bind(stackifyOptions);
            StackifyLib.Config.ApiKey = stackifyOptions.ApiKey;
            StackifyLib.Config.AppName = stackifyOptions.AppName;
            StackifyLib.Config.Environment = stackifyOptions.Environment;

            Configuration = Configuration.WriteTo.Stackify();
        }

        private static void ConfigureSqlLogging(IConfiguration configuration)
        {
            var columnOptions = new ColumnOptions();

            // xml column
            columnOptions.Store.Remove(StandardColumn.Properties);

            // json column
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.LogEvent.ExcludeAdditionalProperties = true;

            // special columns for indexing
            columnOptions.AdditionalDataColumns = new List<DataColumn>()
            {
                new DataColumn {ColumnName = "Source", AllowDBNull = true, DataType = typeof(string), MaxLength = 128},
                new DataColumn {ColumnName = "CorrelationId", AllowDBNull = true, DataType = typeof(string), MaxLength = 50},
                new DataColumn {ColumnName = "JobName", AllowDBNull = true, DataType = typeof(string), MaxLength = 50},
                new DataColumn {ColumnName = "JobId", AllowDBNull = true, DataType = typeof(string), MaxLength = 50},
            };

            Configuration = Configuration
                .WriteTo.MSSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    tableName: "Logs",
                    columnOptions: columnOptions
                );
        }
    }

    public class SqlLogOptions
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
    }

    public class StackifyOptions
    {
        public string AppName { get; set; }
        public string ApiKey { get; set; }
        public string Environment { get; set; }
    }
}
