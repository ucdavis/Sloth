﻿using System;
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
