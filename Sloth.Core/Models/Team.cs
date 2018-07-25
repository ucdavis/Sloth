using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class Team
    {
        public Team()
        {
            ApiKeys = new List<ApiKey>();
            Integrations = new List<Integration>();
            UserTeamRoles = new List<UserTeamRole>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public IList<ApiKey> ApiKeys { get; set; }

        [JsonIgnore]
        public IList<Integration> Integrations { get; set; }

        [JsonIgnore]
        public IList<Source> Sources { get; set; }

        [JsonIgnore]
        public IList<UserTeamRole> UserTeamRoles { get; set; }

        public void AddUserToRole(User user, TeamRole role)
        {
            UserTeamRoles.Add(new UserTeamRole()
            {
                Role = role,
                User = user,
                Team = this,
            });
        }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .ToTable("Teams")
                .HasIndex(t => t.Name).IsUnique();
        }
    }
}
