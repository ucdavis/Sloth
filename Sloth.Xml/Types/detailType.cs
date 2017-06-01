using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    public partial class detailType {
        
        private universityFiscalPeriodCode fiscalPeriodField;
        
        private string fiscalYearField;
        
        private System.DateTime createDateField;
        
        private string chartField;
        
        private string accountField;
        
        private string subAccountField;
        
        private string objectField;
        
        private string subObjectField;
        
        private financialBalanceTypeCode balanceTypeField;
        
        private string objectTypeField;
        
        private string detailSequenceNumField;
        
        private string originCodeField;
        
        private financialDocumentTypeCode docTypeField;
        
        private string docNumField;
        
        private decimal amountField;
        
        private string detailDescriptionField;
        
        /// <remarks/>
        public universityFiscalPeriodCode fiscalPeriod {
            get {
                return this.fiscalPeriodField;
            }
            set {
                this.fiscalPeriodField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="integer")]
        public string fiscalYear {
            get {
                return this.fiscalYearField;
            }
            set {
                this.fiscalYearField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="date")]
        public System.DateTime createDate {
            get {
                return this.createDateField;
            }
            set {
                this.createDateField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string chart {
            get {
                return this.chartField;
            }
            set {
                this.chartField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string account {
            get {
                return this.accountField;
            }
            set {
                this.accountField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string subAccount {
            get {
                return this.subAccountField;
            }
            set {
                this.subAccountField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string @object {
            get {
                return this.objectField;
            }
            set {
                this.objectField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string subObject {
            get {
                return this.subObjectField;
            }
            set {
                this.subObjectField = value;
            }
        }
        
        /// <remarks/>
        public financialBalanceTypeCode balanceType {
            get {
                return this.balanceTypeField;
            }
            set {
                this.balanceTypeField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string objectType {
            get {
                return this.objectTypeField;
            }
            set {
                this.objectTypeField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string detailSequenceNum {
            get {
                return this.detailSequenceNumField;
            }
            set {
                this.detailSequenceNumField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string originCode {
            get {
                return this.originCodeField;
            }
            set {
                this.originCodeField = value;
            }
        }
        
        /// <remarks/>
        public financialDocumentTypeCode docType {
            get {
                return this.docTypeField;
            }
            set {
                this.docTypeField = value;
            }
        }
        
        /// <remarks/>
        public string docNum {
            get {
                return this.docNumField;
            }
            set {
                this.docNumField = value;
            }
        }
        
        /// <remarks/>
        public decimal amount {
            get {
                return this.amountField;
            }
            set {
                this.amountField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string detailDescription {
            get {
                return this.detailDescriptionField;
            }
            set {
                this.detailDescriptionField = value;
            }
        }
    }
}
