using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class User
    {
        public User()
        {
            Keys = new List<ApiKey>();
        }

        [Key]
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        [JsonIgnore]
        public IList<UserRole> UserRoles { get; set; }

        [JsonIgnore]
        public IList<UserTeam> UserTeams { get; set; }

        [JsonIgnore]
        public IList<ApiKey> Keys { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("Users");
        }
    }
}
