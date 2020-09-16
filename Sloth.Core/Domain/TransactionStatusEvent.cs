using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Sloth.Core.Models
{
    public class TransactionStatusEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Status { get; set; }

        public DateTime ValidFromDate { get; set; }

        public DateTime? ValidToDate { get; set; }

        public string EventDetails { get; set; }

        public string TransactionId { get; set; }

        public Transaction Transaction { get; set; }

    }
}
