using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Api.Data
{
    /// <summary>
    /// Creates sample data for development environments
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Create sample data
        /// </summary>
        /// <param name="context"></param>
        public static void Initialize(SlothDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();

            if (!context.ApiKeys.Any())
            {
                CreateApikeys(context);
            }

            if (!context.Scrubbers.Any())
            {
                CreateScrubbers(context);
            }

            context.SaveChanges();
        }

        private static void CreateApikeys(SlothDbContext context)
        {
            var apiKeys = new[]
            {
                new ApiKey() {Id = "TestKey123", Owner = "John", Issued = DateTime.UtcNow},
            };
            context.ApiKeys.AddRange(apiKeys);
        }

        private static void CreateScrubbers(SlothDbContext context)
        {
            var scrubbers = new[]
            {
                new Scrubber(),
            };
            context.Scrubbers.AddRange(scrubbers);
        }
    }
}
