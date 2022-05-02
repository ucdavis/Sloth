using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Sloth.Web.Logging
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

            var loggingSection = _configuration.GetSection("Stackify");

            // standard logger
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // uncomment this to hide EF core general info logs
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", loggingSection.GetValue<string>("AppName"))
                .Enrich.WithProperty("AppEnvironment", loggingSection.GetValue<string>("Environment"))
                .Enrich.WithProperty("Source", "Sloth.Web");

            // various sinks
            logConfig = logConfig
                .WriteTo.Console();

            // add in elastic search sink if the uri is valid
            if (Uri.TryCreate(loggingSection.GetValue<string>("ElasticUrl"), UriKind.Absolute, out var elasticUri))
            {
                logConfig = logConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elasticUri)
                {
                    IndexFormat = "aspnet-sloth-{0:yyyy.MM.dd}"
                });
            }

            return logConfig;
        }

        /// <summary>
        /// Get a logger configuration that logs to both stackify and sql
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetJobConfiguration()
        {
            return GetConfiguration()
                .WriteToSqlCustom();
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
                new SqlColumn {ColumnName = "JobName", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 50},
                new SqlColumn {ColumnName = "JobId", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 50},
            };

            return logConfig
                .WriteTo.MSSqlServer(
                    connectionString: _configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs" },
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
