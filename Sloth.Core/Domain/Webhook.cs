using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class WebHook
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [JsonIgnore]
        public Team Team { get; set; }

        [DisplayName("Enabled")]
        public bool IsActive { get; set; }

        [DisplayName("Payload URL")]
        public string Url { get; set; }

        public string ContentType { get; set; }

        public IList<WebHookRequest> WebHookrequests { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebHook>()
                .HasMany(w => w.WebHookrequests)
                .WithOne(r => r.WebHook)
                .HasForeignKey(r => r.WebHookId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
