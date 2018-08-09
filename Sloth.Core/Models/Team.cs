using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// Campus User ID of main contact responsible for KFS feed.
        /// </summary>
        [MinLength(2)]
        [MaxLength(8)]
        [Required]
        public string KfsContactUserId { get; set; }

        /// <summary>
        /// Email address used to send information when errors occur or other events with the KFS feed.
        /// The email should match the origination code email.
        /// A distribution list is highly recommended.
        /// </summary>
        [MaxLength(40)]
        [EmailAddress]
        [Required]
        public string KfsContactEmail { get; set; }

        /// <summary>
        /// Contact number for personnel/unit responsible for KFS feed.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [RegularExpression("[0-9]*")]
        [Required]
        public string KfsContactPhoneNumber { get; set; }

        /// <summary>
        /// Brief mailing address for Department responsible for KFS feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string KfsContactMailingAddress { get; set; }

        /// <summary>
        /// Department responsible for KFS feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string KfsContactDepartmentName { get; set; }

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
