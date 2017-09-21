using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sloth.Core
{
    internal class MigrationDbContextFactory : IDesignTimeDbContextFactory<SlothDbContext>
    {
        public SlothDbContext CreateDbContext(string[] args)
        {
            return CreateDbContext(
                Directory.GetCurrentDirectory(),
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            );
        }

        private SlothDbContext CreateDbContext(string basePath, string environmentName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connstr = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connstr))
            {
                throw new InvalidOperationException(
                "Could not find a connection string named 'DefaultConnection'.");
            }

            return CreateDbContext(connstr);
        }

        private SlothDbContext CreateDbContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException(
                $"{nameof(connectionString)} is null or empty.",
                nameof(connectionString));

            var optionsBuilder =
             new DbContextOptionsBuilder<SlothDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new SlothDbContext(optionsBuilder.Options);
        }
    }
}
