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
using Sloth.Core;
using Sloth.Core.Data;
using Sloth.Core.Models;
using Sloth.Web.Models;

namespace Sloth.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.OrdinalIgnoreCase);

            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json")
            //    .AddEnvironmentVariables();

            ////only add secrets in development
            //if (isDevelopment)
            //{
            //    builder.AddUserSecrets<Program>();
            //}

            //var configuration = builder.Build();

            //var loggingSection = configuration.GetSection("Stackify");

            //var loggerConfig = new LoggerConfiguration()
            //    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //    // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // uncomment this to hide EF core general info logs
            //    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            //    .Enrich.FromLogContext()
            //    .Enrich.WithExceptionDetails()
            //    .Enrich.WithProperty("Application", loggingSection.GetValue<string>("AppName"))
            //    .Enrich.WithProperty("AppEnvironment", loggingSection.GetValue<string>("Environment"))
            //    .WriteTo.Console()
            //    .WriteTo.Stackify();

            //// add in elastic search sink if the uri is valid
            //Uri elasticUri;
            //if (Uri.TryCreate(loggingSection.GetValue<string>("ElasticUrl"), UriKind.Absolute, out elasticUri))
            //{
            //    loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elasticUri)
            //    {
            //        IndexFormat = "aspnet-tacos-{0:yyyy.MM.dd}"
            //    });
            //}


            var host = BuildWebHost(args);

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
                    Task.Run(() => dbInitializer.Recreate()).Wait();
                }
#endif
                Task.Run(() => dbInitializer.Initialize()).Wait();
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    // Extend default builder to allow loading user secrets when debugging locally in Production mode
                    var env = hostContext.HostingEnvironment;

                    if (!env.IsDevelopment() && string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_LOAD_SECRETS"), "True", StringComparison.OrdinalIgnoreCase))
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));

                        if (appAssembly != null)
                        {
                            config.AddUserSecrets(appAssembly, optional: true);
                        }
                    }
                })
                .UseSerilog()
                .UseStartup<Startup>()
                .Build();
    }
}
