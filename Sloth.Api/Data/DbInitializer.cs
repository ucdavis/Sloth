using System;
using System.Linq;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SlothDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Scrubbers.Any())
            {
                return;
            }

            var scrubbers = new Scrubber[]
            {
                new Scrubber(),
            };
            context.Scrubbers.AddRange(scrubbers);
            context.SaveChanges();
        }
    }
}
