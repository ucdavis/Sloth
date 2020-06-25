using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using StackifyLib;

namespace Sloth.Api.Logging
{
    public static class LoggingConfiguration
    {
        private static bool _loggingSetup;

        private static IConfigurationRoot _configuration;

        /// <summary>
        /// Configure Application Logging
        /// </summary>
        public static void Setup(IConfigurationRoot configuration)
        {
            if (_loggingSetup) return; //only setup logging once

            // save configuration for later calls
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // create global logger with standard configuration
            Log.Logger = GetConfiguration().CreateLogger();

            _loggingSetup = true;
        }

        /// <summary>
        /// Get a logger configuration that logs to stackify
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetConfiguration()
        {
            if (_configuration == null) throw new InvalidOperationException("Call Setup() before requesting a Logger Configuration"); ;

            // standard logger
            var logConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Source", "Sloth.Api");

            // various sinks
            logConfig = logConfig
                .WriteToStackifyCustom();

            return logConfig;
        }

        /// <summary>
        /// Get a logger configuration that logs to both stackify and sql
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetAuditConfiguration()
        {
            return GetConfiguration()
                .WriteToSqlCustom();
        }

        private static LoggerConfiguration WriteToStackifyCustom(this LoggerConfiguration logConfig)
        {
            if (!_loggingSetup)
            {
                _configuration.ConfigureStackifyLogging();
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
            columnOptions.AdditionalColumns = new List<SqlColumn>()
            {
                new SqlColumn {ColumnName = "Source", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 128},
                new SqlColumn {ColumnName = "CorrelationId", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 50},
            };

            return logConfig
                .WriteTo.MSSqlServer(
                    connectionString: _configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new SinkOptions { TableName = "Logs" },
                    restrictedToMinimumLevel: LogEventLevel.Information,
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
