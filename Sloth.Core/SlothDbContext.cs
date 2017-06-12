using System;
using Microsoft.EntityFrameworkCore;
using Sloth.Core.Models;

namespace Sloth.Core
{
    public class SlothDbContext : DbContext
    {
        public SlothDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ApiKey> ApiKeys { get; set; }

        public DbSet<Scrubber> Scrubbers { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}