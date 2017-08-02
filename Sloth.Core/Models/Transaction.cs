using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Core.Models
{
    public class Transaction
    {
        public Transaction()
        {
            Transfers = new List<Transfer>();
        }

        public string Id { get; set; }

        public User Creator { get; set; }

        public TransactionStatus Status { get; set; }

        /// <summary>
        /// Primarily used in Decision Support reporting for additional transaction identification.
        /// Equivalent to the KFS Organization Document Number.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        public string TrackingNumber { get; set; }

        [MinLength(2)]
        public IList<Transfer> Transfers { get; set; }

        public Scrubber Scrubber { get; set; }
    }

    public enum TransactionStatus
    {
        Scheduled,
        Completed,
        Cancelled
    }
}
