using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sloth.Core;
using Sloth.Core.Data;
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
                var dbInitializer = new DbInitializer(context);
#if DEBUG
                if (settings.Value.RebuildDb)
                {
                    Task.Run(() => dbInitializer.Recreate()).Wait();
                }
#endif
                dbInitializer.Initialize();
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
