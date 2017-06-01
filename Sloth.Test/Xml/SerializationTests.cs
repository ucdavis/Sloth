using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sloth.Xml;

namespace Sloth.Test.Xml
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void TestSimpleSerialzation()
        {
            var xs = new XmlSerializer(typeof(Batch));
            var sw = new StringWriter();

            var target = new Batch()
            {
                Header = new Header()
                {
                    batchDate = new DateTime(2000, 01, 01)
                },
                Detail = new[]
                {
                    new Detail()
                    {
                        
                    }, 
                },
                Entries = new[]
                {
                    new Entry()
                    {
                        
                    }, 
                },
                Trailer = new Trailer()
                {
                    
                },
            };
            
            // act
            xs.Serialize(sw, target);

            // assert
            var actual = sw.ToString();
            Assert.AreEqual(actual, "");
        }
    }
}
