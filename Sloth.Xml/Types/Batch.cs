using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlRoot("batch", Namespace = "http://www.kuali.org/kfs/gl/collector", IsNullable = false)]
    [XmlType("batchType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Batch
    {
        /// <remarks/>
        [XmlElement("header", Order = 1)]
        public Header Header { get; set; }

        /// <remarks/>
        [XmlElement("glEntry", Order = 2)]
        public Entry[] Entries { get; set; }

        /// <remarks/>
        [XmlElement("detail", Order = 3)]
        public Detail[] Detail { get; set; }

        /// <remarks/>
        [XmlElement("trailer", Order = 4)]
        public Trailer Trailer { get; set; }
    }
}
