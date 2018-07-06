using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class Team
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public IList<Integration> Integrations { get; set; }

        [JsonIgnore]
        public IList<UserTeamRole> UserTeamRoles { get; set; }

        [JsonIgnore]
        public IList<ApiKey> ApiKeys { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .ToTable("Teams")
                .HasIndex(t => t.Name).IsUnique();
        }
    }
}
