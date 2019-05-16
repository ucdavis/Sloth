using System;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class ApiKey
    {
        public ApiKey()
        {
            Key = Guid.NewGuid().ToString("N");
            Issued = DateTime.UtcNow;
        }

        public string Id { get; set; }
        public string Key { get; set; }
        public Team Team { get; set; }
        public DateTime Issued { get; set; }
        public DateTime? Revoked { get; set; }

        #region Helper Methods

        public string MaskedKey()
        {
            var len = Key.Length;
            var left = Key.Substring(0, 4);
            var right = Key.Substring(len - 4);
            return left.PadRight(len - 4, '*') + right;
        }
        #endregion

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApiKey>()
                .ToTable("ApiKeys")
                .HasIndex(k => k.Key).IsUnique();
        }
    }
}
