using System;
using System.Threading;
using Hangfire;
using Hangfire.Console;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Api;
using Sloth.Api.Jobs;
using Sloth.Api.Services;
using Sloth.Core;
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
                var serviceProvider = BuildServiceProvider();

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

        private static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            // expose configuration
            services.AddSingleton<IConfiguration>(_ => Configuration);

            // add database
            services.AddDbContext<SlothDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // add infrastructure services
            services.AddTransient<IStorageService, StorageService>();

            // add logging setup
            services.AddTransient(s => LoggingConfiguration.Configuration);

            // add jobs
            services.AddTransient<Heartbeat>();
            services.AddTransient<UploadScrubberJob>();

            return services.BuildServiceProvider();
        }

        private static IConfigurationRoot Configuration { get; set; }
    }
}