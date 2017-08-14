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

        public string Id { get; set; }

        public string Uri { get; set; }

        /// <summary>
        /// Chart of Accounts Code associated with Org Code
        /// </summary>
        [MaxLength(2)]
        [Required]
        public string Chart { get; set; }

        /// <summary>
        /// Financial System Organization responsible for GL Feed.
        /// </summary>
        [MaxLength(4)]
        [Required]
        public string OrganizationCode { get; set; }

        /// <summary>
        /// Date this file was created/submitted for batching
        /// </summary>
        [Required]
        public DateTime BatchDate { get; set; }

        /// <summary>
        /// Source system batch sequence number
        /// </summary>
        [Range(1, 999999)]
        [Required]
        public int BatchSequenceNumber { get; set; }

        /// <summary>
        /// </summary>
        [MinLength(1)]
        [Required]
        public IList<Transaction> Transactions { get; set; }
    }
}
