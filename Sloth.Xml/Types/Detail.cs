using System;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlRoot("detail", Namespace = KfsNamespace, IsNullable = false)]
    [XmlType("detailType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Detail: KfsXmlElement
    {
        /// <remarks/>
        [XmlElement("fiscalPeriod")]
        public UniversityFiscalPeriodCode FiscalPeriod { get; set; }

        /// <remarks/>
        [XmlElement("fiscalYear", DataType = "integer")]
        public string FiscalYear { get; set; }

        /// <remarks/>
        [XmlElement("createDate", DataType = "date")]
        public DateTime CreateDate { get; set; }

        /// <remarks/>
        [XmlElement("chart", DataType = "normalizedString")]
        public string Chart { get; set; }

        /// <remarks/>
        [XmlElement("account", DataType = "normalizedString")]
        public string Account { get; set; }

        /// <remarks/>
        [XmlElement("subAccount", DataType = "normalizedString")]
        public string SubAccount { get; set; }

        /// <remarks/>
        [XmlElement("object", DataType = "normalizedString")]
        public string ObjectCode { get; set; }

        /// <remarks/>
        [XmlElement("subObject", DataType = "normalizedString")]
        public string SubObject { get; set; }

        /// <remarks/>
        [XmlElement("balanceType")]
        public FinancialBalanceTypeCode BalanceType { get; set; }

        /// <remarks/>
        [XmlElement("objectType", DataType = "normalizedString")]
        public string ObjectType { get; set; }

        /// <remarks/>
        [XmlElement("detailSequenceNum", DataType = "normalizedString")]
        public string DetailSequenceNumber { get; set; }

        /// <remarks/>
        [XmlElement("originCode", DataType = "normalizedString")]
        public string OriginCode { get; set; }

        /// <remarks/>
        [XmlElement("docType")]
        public FinancialDocumentTypeCode DocType { get; set; }

        /// <remarks/>
        [XmlElement("docNum")]
        public string DocNum { get; set; }

        /// <remarks/>
        [XmlElement("amount")]
        public decimal Amount { get; set; }

        /// <remarks/>
        [XmlElement("detailDescription", DataType = "normalizedString")]
        public string DetailDescription { get; set; }
    }
}
