using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Core.Models
{
    public class JournalRequest
    {
        public JournalRequest()
        {
            CreatedAt = DateTime.UtcNow;
            Transactions = new List<Transaction>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required] public Source Source { get; set; }

        /// <summary>
        /// GL Journal Request ID from Aggie Enterprise
        /// </summary>
        [Required]
        public Guid RequestId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Status of the journal request at the time of creation
        /// </summary>
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// </summary>
        [MinLength(1)]
        [Required]
        public IList<Transaction> Transactions { get; set; }

        /// <summary>
        /// Really, a journal request can only have 1 transaction, but if we re-send a JR we loose the tie to the original transaction
        /// This doesn't have to be a foreign key, but it should be a unique identifier for the transaction
        /// </summary>
        [MaxLength(255)]
        public string SavedTransactionId { get; set; }
    }
}
