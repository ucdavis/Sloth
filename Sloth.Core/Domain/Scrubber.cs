using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Core.Models
{
    public class Scrubber
    {
        public Scrubber()
        {
            Transactions = new List<Transaction>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Uri { get; set; }

        /// <summary>
        /// Date this file was created/submitted for batching
        /// </summary>
        [Display(Name = "Batch Date")]
        [Required]
        public DateTime BatchDate { get; set; }

        /// <summary>
        /// Source system batch sequence number
        /// </summary>
        [Range(1, 999999)]
        [Required]
        public int BatchSequenceNumber { get; set; }

        [Required]
        public Source Source { get; set; }

        /// <summary>
        /// </summary>
        [MinLength(1)]
        [Required]
        public IList<Transaction> Transactions { get; set; }

        public string BlobId { get; set; }

        public Blob Blob { get; set; }


    }
}
