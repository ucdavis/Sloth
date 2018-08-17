using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Sloth.Xml;
using Sloth.Xml.Types;
using Xunit;

namespace Sloth.Test.Xml
{
    public class SerializationTests
    {
        [Fact]
        public void TestSimpleSerialzation()
        {
            // setup
            var target = new Batch
            {
                Header = new Header()
            };

            target.Entries.Add(new Entry());
            target.Entries.Add(new EntryWithDetail(){ Detail = new Detail() });
            target.Entries.Add(new EntryWithDetail(){ Detail = new Detail() });

            target.Trailer = new Trailer()
            {
                TotalAmount = target.Entries.Sum(e => e.Amount),
                TotalRecords = target.Entries.Count
            };


            // act
            var xs = new XmlSerializer(typeof(Batch));
            var sw = new StringWriter();

            var xwx = new XmlWriterSettings()
            {
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8,
                Indent = true
            };
            var xw = XmlWriter.Create(sw, xwx);

            xs.Serialize(xw, target);

            // assert
            var actual = sw.ToString();
            Assert.NotEmpty(actual);
        }

        [Fact]
        public void TestSimpleDeserialization()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var sr = new StreamReader(baseDir + "/Data/sample.xml", Encoding.UTF8);

            // act
            var xs = new XmlSerializer(typeof(Batch));
            var actual = (Batch)xs.Deserialize(sr);

            // check structure
            Assert.NotNull(actual);
            Assert.NotNull(actual.Header);
            Assert.Equal(13, actual.Entries.Count);
            Assert.NotNull(actual.Trailer);

            // check header
            var header = actual.Header;
            Assert.Equal("3", header.Chart);
            Assert.Equal("ACCT", header.OrganizationCode);
            Assert.Equal(new DateTime(2017, 05, 08), header.BatchDate);
            Assert.Equal(2, header.BatchSequenceNumber);
            Assert.Equal("jackiev", header.ContactUserId);
            Assert.Equal("javelasquez@ucdavis.edu", header.ContactEmail);
            Assert.Equal(CampusCode.DV, header.CampusCode);
            Assert.Equal("5307578571", header.ContactPhoneNumber);
            Assert.Equal("1441 Research Park Drive", header.MailingAddress);
            Assert.Equal("AdminIT", header.DepartmentName);

            // check entries
            var entry = actual.Entries[0];
            Assert.Equal(2017, entry.FiscalYear);
            Assert.Equal("3", entry.Chart);
            Assert.Equal("6620001", entry.Account);
            Assert.Equal("7259", entry.ObjectCode);
            Assert.Equal(FinancialBalanceTypeCode.AC, entry.BalanceType);
            Assert.Equal(UniversityFiscalPeriodCode.Period11, entry.FiscalPeriod);
            Assert.Equal(FinancialDocumentTypeCode.GLJV, entry.DocType);
            Assert.Equal("92", entry.OriginCode);
            Assert.Equal("ADOCUMENT1", entry.DocumentNumber);
            Assert.Equal(1, entry.SequenceNumber);
            Assert.Equal("Some useful description", entry.Description);
            Assert.Equal((decimal) 162.22, entry.Amount);
            Assert.Equal(TransactionDebitCreditCode.Debit, entry.DebitCredit);
            Assert.Equal(new DateTime(2017, 05, 01), entry.TransactionDate);
            Assert.Equal("TESTTHIS1", entry.TrackingNumber);

            // check trailer
            var trailer = actual.Trailer;
            Assert.Equal(13, trailer.TotalRecords);
            Assert.Equal((decimal) 2031.20, trailer.TotalAmount);
        }
    }
}
