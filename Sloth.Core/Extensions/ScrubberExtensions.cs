using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Sloth.Core.Models;
using Sloth.Xml;

namespace Sloth.Core.Extensions
{
    public static class ScrubberExtensions
    {
        public static string ToXml(this Scrubber scrubber)
        {
            var sw = new StringWriter();
            ToXml(scrubber, sw);
            return sw.ToString();
        }

        public static void ToXml(this Scrubber scrubber, TextWriter output)
        {
            var transfers = scrubber.Transactions.SelectMany(t => t.Transfers).ToList();

            var batch = new Batch
            {
                Header = new Header()
                {
                    BatchDate           = scrubber.BatchDate,
                    BatchSequenceNumber = scrubber.BatchSequenceNumber,
                    Chart               = scrubber.Chart,
                    CampusCode          = campusCode.DV,
                    OrganizationCode    = scrubber.OrganizationCode,
                },
                Entries = transfers.Select(t => t.ToEntry()).ToList(),
                Trailer = new Trailer()
                {
                    TotalAmount = transfers.Where(t => t.Direction == Transfer.CreditDebit.Credit).Sum(t => t.Amount),
                    TotalRecords = transfers.Count
                }
            };

            var xs = new XmlSerializer(typeof(Batch));
            var xwx = new XmlWriterSettings()
            {
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8,
                Indent = true
            };
            var xw = XmlWriter.Create(output, xwx);

            xs.Serialize(xw, batch);
        }
    }
}
