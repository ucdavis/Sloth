using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }

        [JsonIgnore]
        public string EmailHash
        {
            get
            {
                // Create a new instance of the MD5CryptoServiceProvider object.
                var md5Hasher = MD5.Create();

                // Convert the input string to a byte array and compute the hash.
                var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(Email.ToLower().Trim()));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString(); 
            }
        }

        [JsonIgnore]
        public IList<UserTeamRole> UserTeamRoles { get; set; }

        public IEnumerable<Team> GetTeams()
        {
            return UserTeamRoles.Select(p => p.Team).Distinct();
        }

        public bool IsTeamAdmin(string teamName)
        {
            return UserTeamRoles.Any(a => a.Team.Name == teamName && a.Role.Name == TeamRole.Admin);
        }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
