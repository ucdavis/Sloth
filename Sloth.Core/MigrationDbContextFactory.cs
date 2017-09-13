using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Sloth.Core
{
    internal class MigrationDbContextFactory : IDesignTimeDbContextFactory<SlothDbContext>
    {
        public SlothDbContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SlothDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=sloth;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new SlothDbContext(optionsBuilder.Options);
        }

        public SlothDbContext CreateDbContext(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
