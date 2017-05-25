using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sloth.Core.Models;

namespace Sloth.Core
{
    public partial class SlothDbContext : DbContext
    {
        public SlothDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Scrubber> Scrubbers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}