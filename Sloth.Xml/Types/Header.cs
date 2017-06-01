using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("headerType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Header
    {
        /// <remarks/>
        [MaxLength(2)]
        [XmlElement("chart", DataType = "normalizedString")]
        public string chart { get; set; }

        /// <remarks/>
        [MaxLength(4)]
        [XmlElement("orgCode", DataType = "normalizedString")]
        public string orgCode { get; set; }

        /// <remarks/>
        [XmlElement("batchDate", DataType = "date")]
        public DateTime batchDate { get; set; }

        /// <remarks/>
        [XmlElement("batchSequenceNum", DataType = "positiveInteger")]
        public string batchSequenceNum { get; set; }

        /// <remarks/>
        [XmlElement("contactUserId", DataType = "normalizedString")]
        public string contactUserId { get; set; }

        /// <remarks/>
        [XmlElement("contactEmail", DataType = "normalizedString")]
        public string contactEmail { get; set; }

        /// <remarks/>
        [XmlElement("campusCode")]
        public campusCode campusCode { get; set; }

        /// <remarks/>
        [XmlElement("contactPhoneNumber", DataType = "normalizedString")]
        public string contactPhoneNumber { get; set; }

        /// <remarks/>
        [XmlElement("mailingAddress", DataType = "normalizedString")]
        public string mailingAddress { get; set; }

        /// <remarks/>
        [XmlElement("departmentName", DataType = "normalizedString")]
        public string departmentName { get; set; }
    }
}
