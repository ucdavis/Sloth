using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Sloth.Core.Domain;

namespace Sloth.Core.Models
{
    public class Blob
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        public string Uri { get; set; }

        public string Description { get; set; }

        public string Container { get; set; }

        [Display(Name = "Media Type")]
        public string MediaType { get; set; }

        [Display(Name = "Uploaded Date")]
        public DateTime UploadedDate { get; set; }

        public IList<Scrubber> Scrubbers { get; set; }

        public IList<CybersourceBankReconcileJobBlob> CybersourceBankReconcileJobBlobs { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blob>()
                .HasMany(blob => blob.Scrubbers)
                .WithOne(s => s.Blob)
                .HasForeignKey(s => s.BlobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Blob>()
                .HasMany(blob => blob.CybersourceBankReconcileJobBlobs)
                .WithOne(r => r.Blob)
                .HasForeignKey(r => r.BlobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
