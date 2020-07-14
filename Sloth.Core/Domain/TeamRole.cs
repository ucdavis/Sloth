using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class TeamRole
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamRole>()
                .HasIndex(r => r.Name).IsUnique();
        }

        // Role Codes
        public const string Admin = "Admin";
        public const string Approver = "Approver";

        public static string[] GetAllRoles()
        {
            return new[]
            {
                Admin,
                Approver,
            };
        }
    }
}
