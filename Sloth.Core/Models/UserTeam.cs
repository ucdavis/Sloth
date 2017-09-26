using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class UserTeam
    {
        public string UserId { get; set; }

        public User User { get; set; }

        public string TeamId { get; set; }

        public Team Team { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTeam>()
                .ToTable("UserTeams")
                .HasKey(t => new { t.UserId, t.TeamId });
        }
    }
}
