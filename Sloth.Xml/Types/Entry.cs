using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlRoot("entry", Namespace = KfsNamespace, IsNullable = false)]
    [XmlType("glEntryType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Entry : KfsXmlElement
    {
        /// <remarks/>
        [XmlElement("fiscalYear", DataType = "integer")]
        public string fiscalYear { get; set; }

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
        [XmlElement("fiscalPeriod")]
        public universityFiscalPeriodCode fiscalPeriod { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool fiscalPeriodSpecified { get; set; }

        /// <remarks/>
        [XmlElement("docType")]
        public financialDocumentTypeCode docType { get; set; }

        /// <remarks/>
        [XmlElement("originCode", DataType = "normalizedString")]
        public string originCode { get; set; }

        /// <remarks/>
        [XmlElement("docNum")]
        public string docNum { get; set; }

        /// <remarks/>
        [XmlElement("sequenceNum", DataType = "positiveInteger")]
        public string sequenceNum { get; set; }

        /// <remarks/>
        [XmlElement("description", DataType = "normalizedString")]
        public string description { get; set; }

        /// <remarks/>
        [XmlElement("amount")]
        public decimal amount { get; set; }

        /// <remarks/>
        [XmlElement("debitCredit")]
        public transactionDebitCreditCode debitCredit { get; set; }

        /// <remarks/>
        [XmlElement("transDate", DataType = "date")]
        public System.DateTime transDate { get; set; }

        /// <remarks/>
        [XmlElement("trackingNumber", DataType = "normalizedString")]
        public string trackingNumber { get; set; }

        /// <remarks/>
        [XmlElement("project", DataType = "normalizedString")]
        public string project { get; set; }

        /// <remarks/>
        [XmlElement("referenceId", DataType = "normalizedString")]
        public string referenceId { get; set; }

        /// <remarks/>
        [XmlElement("refDocType")]
        public financialDocumentTypeCode refDocType { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool refDocTypeSpecified { get; set; }

        /// <remarks/>
        [XmlElement("refOriginCode", DataType = "normalizedString")]
        public string refOriginCode { get; set; }

        /// <remarks/>
        [XmlElement("refDocNum")]
        public string refDocNum { get; set; }

        /// <remarks/>
        [XmlElement("reveralDate", DataType = "date")]
        public System.DateTime reversalDate { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool reversalDateSpecified { get; set; }

        /// <remarks/>
        [XmlElement("encumbCode")]
        public transactionEncumbranceUpdateCode encumbCode { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool encumbCodeSpecified { get; set; }
    }
}
