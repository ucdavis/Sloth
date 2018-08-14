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

            target.Entries.Add(new EntryWithDetail());
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

            // assert
            Assert.NotNull(actual);
            Assert.NotNull(actual.Header);
            Assert.Equal(actual.Entries.Count, 13);
            Assert.NotNull(actual.Trailer);
        }
    }
}
