using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class Role
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .ToTable("Roles")
                .HasIndex(r => r.Name).IsUnique();
        }
    }
}
