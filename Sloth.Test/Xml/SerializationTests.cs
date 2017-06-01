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
            var xs = new XmlSerializer(typeof(batchType));
            var sw = new StringWriter();

            var target = new batchType()
            {
                header = new headerType()
                {
                    batchDate = new DateTime(2000, 01, 01)
                },
                detail = new[]
                {
                    new detailType()
                    {
                        
                    }, 
                },
                glEntry = new[]
                {
                    new glEntryType()
                    {
                        
                    }, 
                },
                trailer = new trailerType()
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
