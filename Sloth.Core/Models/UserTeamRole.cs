using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class UserTeamRole
    {
        public string UserId { get; set; }

        public User User { get; set; }

        public string RoleId { get; set; }

        public Role Role { get; set; }

        public string TeamId { get; set; }

        public Team Team { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTeamRole>()
                .ToTable("UserTeamRoles")
                .HasKey(t => new {t.UserId, t.RoleId, t.TeamId});
        }
    }
}
