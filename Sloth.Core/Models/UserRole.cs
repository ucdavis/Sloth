using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class UserRole
    {
        public string UserId { get; set; }

        public User User { get; set; }

        public string RoleId { get; set; }

        public Role Role { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .ToTable("UserRoles")
                .HasKey(t => new { t.UserId, t.RoleId});
        }
    }
}
