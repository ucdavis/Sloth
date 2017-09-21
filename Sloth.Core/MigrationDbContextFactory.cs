using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Sloth.Core
{
    internal class MigrationDbContextFactory : IDbContextFactory<SlothDbContext>
    {
        public SlothDbContext Create(DbContextFactoryOptions options)
        {
            return Create(
                options.ContentRootPath,
                options.EnvironmentName);
        }

        private SlothDbContext Create(string basePath, string environmentName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connstr = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connstr))
            {
                throw new InvalidOperationException(
                "Could not find a connection string named 'DefaultConnection'.");
            }

            return Create(connstr);
        }

        private SlothDbContext Create(string connectionString)
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
