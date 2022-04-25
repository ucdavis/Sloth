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

        public string RequestStatus { get;set;} = string.Empty; //AE status 
        /*
PENDING

Request has been submitted to the server, but not validated or processed.
INPROCESS

Request has been picked up for processing
ERROR

There was an error processing the request after it was picked up.
PROCESSED

Request has been processed, but the callback has not been completed.
COMPLETE

If Callback URL Provided: Request has been processed, and the callback was successfully contacted. Or, request has been processed, and no callback URL was provided.
STALE

If Callback URL Provided: Request has been processed, but repeated attempts to contact the callback have failed and no more will be tried.
REJECTED

There was a validation error in the request payload data.
         */
        public DateTime? LastStatusDateTime { get; set; }
        public DateTime? ProcessedDateTime { get; set;}
        public string ErrorMessages { get; set; } = string.Empty;

    }
}
