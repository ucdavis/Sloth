using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("trailerType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Trailer
    {
        /// <remarks/>
        [XmlElement("totalRecords", DataType = "positiveInteger")]
        public string totalRecords { get; set; }

        /// <remarks/>
        [XmlElement("totalAmount")]
        public decimal totalAmount { get; set; }
    }
}
