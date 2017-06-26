using System;
using System.Threading;
using Hangfire;
using Hangfire.Console;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Api;
using Sloth.Api.Jobs;
using Sloth.Jobs.HangfireServer.Logging;

namespace Sloth.Jobs.HangfireServer
{
    public class Program
    {
        private static BackgroundJobServerOptions _options;
        private static CancellationToken _cancellationToken;

        public static void Main(string[] args)
        {
            try
            {
                // build app settings configuration
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets<Startup>();

                Configuration = builder.Build();

                // configure logging
                LoggingConfiguration.Setup(Configuration);

                // configure DI
                var serviceProvider = new ServiceCollection()
                    .AddTransient(s => LoggingConfiguration.Configuration)
                    .AddTransient<Heartbeat>()
                    .BuildServiceProvider();

                // configure handfire job processor
                GlobalConfiguration.Configuration
                    .UseConsole()
                    .UseSerilogLogProvider()
                    .UseActivator(new ServiceActivator(serviceProvider))
                    .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));

                // setup action filters
                GlobalJobFilters.Filters.Add(new JobContextLoggerAttribute());

                // configure job server
                _options = new BackgroundJobServerOptions()
                {
                };

                // listen for shutdon
                _cancellationToken = new WebJobsShutdownWatcher().Token;

                // startup job server
                using (var server = new BackgroundJobServer(_options))
                {
                    while (true)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            server.SendStop();
                            break;
                        }

                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        private static IConfigurationRoot Configuration { get; set; }
    }
}