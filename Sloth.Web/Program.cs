using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Sloth.Core;
using Sloth.Core.Data;
using Sloth.Core.Models;
using Sloth.Web.Logging;
using Sloth.Web.Models;

namespace Sloth.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
#if DEBUG
            Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));
#endif
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();

            //only add secrets in development or when debugging locally in Production mode...
            if (isDevelopment || string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_LOAD_SECRETS"), "True", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddUserSecrets<Program>();
            }

            var configuration = builder.Build();

            LoggingConfiguration.Setup(configuration);

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var settings = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
                var context = scope.ServiceProvider.GetRequiredService<SlothDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var dbInitializer = new DbInitializer(context, userManager, roleManager);
#if DEBUG
                if (settings.Value.RebuildDb)
                {
                    await dbInitializer.Recreate();
                }
#endif
                if (settings.Value.AutoMigrateDb)
                {
                    await dbInitializer.Migrate();
                }
                await dbInitializer.Initialize();
            }

            try
            {
                Log.Information("Starting up");
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
