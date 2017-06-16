using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlRoot("trailer", Namespace = KfsNamespace, IsNullable = false)]
    [XmlType("trailerType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Trailer : KfsXmlElement
    {
        /// <summary>
        /// The total number of GL Entry Tags
        /// </summary>
        [Range(1, 1000000)]
        [Required]
        [XmlElement("totalRecords", DataType = "int")]
        public int TotalRecords { get; set; }

        /// <summary>
        /// Total amount of the transactions being processed in a file.
        /// </summary>
        [Range(typeof(decimal), "0.01", "1000000000")]
        [Required]
        [XmlElement("totalAmount")]
        public decimal TotalAmount { get; set; }
    }
}
