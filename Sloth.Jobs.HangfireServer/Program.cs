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
using Sloth.Api.Jobs.Attributes;
using Sloth.Api.Logging;
using Sloth.Api.Services;
using Sloth.Core;

namespace Sloth.Jobs.HangfireServer
{
    public class Program
    {
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

                // listen for shutdown
                _cancellationToken = new WebJobsShutdownWatcher().Token;

                // configure DI
                var serviceProvider = BuildServiceProvider();

                // configure handfire job processor
                GlobalConfiguration.Configuration
                    .UseConsole()
                    .UseSerilogLogProvider()
                    .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));

                // setup action filters
                GlobalJobFilters.Filters.Add(new JobContextLoggerAttribute());

                // configure job server
                var options = new BackgroundJobServerOptions()
                {
                    Activator = new ServiceActivator(serviceProvider),
                };

                // startup job server
                using (var server = new BackgroundJobServer(options))
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
            services.AddTransient<IKfsScrubberService, KfsScrubberService>();
            services.AddTransient<IStorageService, StorageService>();

            // add jobs
            services.AddTransient<Heartbeat>();
            services.AddTransient<UploadScrubberJob>();

            return services.BuildServiceProvider();
        }

        private static IConfigurationRoot Configuration { get; set; }
    }
}