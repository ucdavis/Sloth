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
                .UseStartup<Startup>()
                .Build();
    }
}
