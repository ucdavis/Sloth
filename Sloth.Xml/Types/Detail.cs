using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("detailType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Detail
    {
        /// <remarks/>
        [XmlElement("fiscalPeriod")]
        public universityFiscalPeriodCode fiscalPeriod { get; set; }

        /// <remarks/>
        [XmlElement("fiscalYear", DataType = "integer")]
        public string fiscalYear { get; set; }

        /// <remarks/>
        [XmlElement("createDate", DataType = "date")]
        public System.DateTime createDate { get; set; }

        /// <remarks/>
        [XmlElement("chart", DataType = "normalizedString")]
        public string chart { get; set; }

        /// <remarks/>
        [XmlElement("account", DataType = "normalizedString")]
        public string account { get; set; }

        /// <remarks/>
        [XmlElement("subAccount", DataType = "normalizedString")]
        public string subAccount { get; set; }

        /// <remarks/>
        [XmlElement("object", DataType = "normalizedString")]
        public string objectCode { get; set; }

        /// <remarks/>
        [XmlElement("subObject", DataType = "normalizedString")]
        public string subObject { get; set; }

        /// <remarks/>
        [XmlElement("balanceType")]
        public financialBalanceTypeCode balanceType { get; set; }

        /// <remarks/>
        [XmlElement("objectType", DataType = "normalizedString")]
        public string objectType { get; set; }

        /// <remarks/>
        [XmlElement("detailSequenceNum", DataType = "normalizedString")]
        public string detailSequenceNum { get; set; }

        /// <remarks/>
        [XmlElement("originCode", DataType = "normalizedString")]
        public string originCode { get; set; }

        /// <remarks/>
        [XmlElement("docType")]
        public financialDocumentTypeCode docType { get; set; }

        /// <remarks/>
        [XmlElement("docNum")]
        public string docNum { get; set; }

        /// <remarks/>
        [XmlElement("amount")]
        public decimal amount { get; set; }

        /// <remarks/>
        [XmlElement("detailDescription", DataType = "normalizedString")]
        public string detailDescription { get; set; }
    }
}
