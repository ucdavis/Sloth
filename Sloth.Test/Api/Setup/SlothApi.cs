using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Test.Helpers;

public class SlothApi : WebApplicationFactory<Sloth.Api.Startup>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Can't use SQlite because it doesn't support sequences
        var root = new InMemoryDatabaseRoot();

        var projectDir = Directory.GetCurrentDirectory();
        var configPath = Path.Combine(projectDir, "appsettings-test.json");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(configPath, optional: false);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<SlothDbContext>));
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<SlothDbContext>()
                .AddUserManager<TestApplicationUserManager>();
            services.AddDbContext<SlothDbContext>(options =>
                options.UseInMemoryDatabase("SlothTesting", root));
        });

        return base.CreateHost(builder);
    }
}
