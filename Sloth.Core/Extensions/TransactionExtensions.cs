using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Utilities;

namespace Sloth.Core.Extensions
{
    public static class TransactionExtensions
    {
        public static bool IsStale(this Transaction transaction)
        {
            var lastProcessingEvent = transaction.StatusEvents
                .Where(e => e.Status == TransactionStatuses.Processing)
                .OrderByDescending(e => e.EventDate)
                .FirstOrDefault();

            // a missing last processing event means the transaction was likely created
            // prior to the implementation of status events
            return transaction.Status == TransactionStatuses.Processing
                 && (lastProcessingEvent == null
                 || lastProcessingEvent.EventDate < DateTime.UtcNow.Date.AddDays(-5));
        }

        public static string ToXml(this Transaction transaction)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);
            ToXml(transaction, sw);
            return sw.ToString();
        }

        public static void ToXml(this Transaction transaction, TextWriter output)
        {
            // create fake scrubber to serialize
            var scrubber = new Scrubber()
            {
                BatchDate           = DateTime.Today,
                BatchSequenceNumber = 1,
                Transactions        = new List<Transaction>() { transaction },
                Source              = transaction.Source
            };

            scrubber.ToXml(output);
        }
    }
}
