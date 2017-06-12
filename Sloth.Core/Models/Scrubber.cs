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

        public User Creator { get; set; }

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
        /// Campus User ID of main contact responsible for feed.
        /// </summary>
        [MinLength(2)]
        [MaxLength(8)]
        [Required]
        public string ContactUserId { get; set; }

        /// <summary>
        /// Email address used to send information when errors occur or other events.
        /// The email should match the origination code email.
        /// A distribution list is highly recommended.
        /// </summary>
        [MaxLength(40)]
        [EmailAddress]
        [Required]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Contact number for personnel/unit responsible for feed.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [RegularExpression("[0-9]*")]
        [Required]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Brief mailing address for Department responsible for feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string ContactAddress { get; set; }

        /// <summary>
        /// Department responsible for feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string ContactDepartment { get; set; }

        /// <summary>
        /// Financial System only code. Value to be provided by Admin IT to the integration system.
        /// </summary>
        [MaxLength(2)]
        [Required]
        public string CampusCode { get; set; }

        /// <summary>
        /// </summary>
        [MinLength(1)]
        [Required]
        public IList<Transaction> Transactions { get; set; }
    }
}
