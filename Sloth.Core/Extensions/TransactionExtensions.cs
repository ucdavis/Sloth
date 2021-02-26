using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sloth.Core.Models;
using Sloth.Core.Utilities;

namespace Sloth.Core.Extensions
{
    public static class TransactionExtensions
    {
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
