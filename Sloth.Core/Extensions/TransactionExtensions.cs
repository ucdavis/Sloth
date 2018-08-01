using System;
using System.Collections.Generic;
using System.IO;
using Sloth.Core.Models;

namespace Sloth.Core.Extensions
{
    public static class TransactionExtensions
    {
        public static string ToXml(this Transaction transaction)
        {
            var sw = new StringWriter();
            ToXml(transaction, sw);
            return sw.ToString();
        }

        public static void ToXml(this Transaction transaction, TextWriter output)
        {
            var source = transaction.Source;

            var chart = source.Chart;
            var orgCode = source.OrganizationCode;
            var originCode = source.OriginCode;
            var docType = source.DocumentType;

            // create scrubber
            var scrubber = new Scrubber()
            {
                Chart = chart,
                OrganizationCode = orgCode,
                BatchDate = DateTime.Today,
                BatchSequenceNumber = 1,
                Transactions = new List<Transaction>() { transaction },
                OriginCode = originCode,
                DocumentType = docType
            };
        }
    }
}
