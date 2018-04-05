using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.MSSqlServer;
using StackifyLib;

namespace Sloth.Api.Logging
{
    public static class LoggingConfiguration
    {
        private static bool _loggingSetup;

        private static LoggerConfiguration _loggerConfiguration;

        /// <summary>
        /// Configure Application Logging
        /// </summary>
        public static void Setup(IConfigurationRoot configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (_loggingSetup) return; //only setup logging once

            if (true)
            {
                Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            }

            // standard logger
            _loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Source", "Sloth.Api");

            // various sinks
            ConfigureSqlLogging(configuration);

            ConfigureStackifyLogging(configuration);

            // create global logger
            Log.Logger = _loggerConfiguration.CreateLogger();

            _loggingSetup = true;
        }

        private static void ConfigureStackifyLogging(IConfigurationRoot configuration)
        {
            configuration.ConfigureStackifyLogging();
            
            _loggerConfiguration = _loggerConfiguration.WriteTo.Stackify();
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
            };

            _loggerConfiguration = _loggerConfiguration
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
