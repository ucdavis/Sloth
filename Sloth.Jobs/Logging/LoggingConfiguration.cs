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

        private static IConfiguration _configuration;

        /// <summary>
        /// Configure Global Application Logging
        /// </summary>
        public static void Setup(IConfiguration configuration)
        {
            if (_loggingSetup) return; //only setup logging once

            // save configuration for later calls
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // create global logger
            Log.Logger = GetConfiguration().CreateLogger();

            _loggingSetup = true;
        }

        /// <summary>
        /// Get another copy of the logger configuration
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetConfiguration()
        {
            if (_configuration == null) throw new InvalidOperationException("Call Setup() before requesting a Logger Configuration");;

            // standard logger
            var logConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Source", "Sloth.Jobs");

            // various sinks
            logConfig = logConfig
                .WriteToStackifyCustom()
                .WriteToSqlCustom();

            return logConfig;
        }

        private static LoggerConfiguration WriteToStackifyCustom(this LoggerConfiguration logConfig)
        {
            if (true)
            {
                Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            }

            return logConfig.WriteTo.Stackify();
        }

        private static LoggerConfiguration WriteToSqlCustom(this LoggerConfiguration logConfig)
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

            return logConfig
                .WriteTo.MSSqlServer(
                    connectionString: _configuration.GetConnectionString("DefaultConnection"),
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
