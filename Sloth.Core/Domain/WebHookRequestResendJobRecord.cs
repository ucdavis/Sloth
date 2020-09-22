using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Sloth.Core.Domain;

namespace Sloth.Core.Models
{
    public class WebHookRequestResendJobRecord : JobRecord
    {
        public IList<WebHookRequest> WebHookRequests { get; set; }

        public static string JobName = "WebHooks.Resend";

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebHookRequestResendJobRecord>()
                .HasMany(r => r.Logs)
                .WithOne()
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WebHookRequestResendJobRecord>()
                .HasMany(j => j.WebHookRequests)
                .WithOne(j => j.WebHookRequestResendJob)
                .HasForeignKey(j => j.WebHookRequestResendJobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
