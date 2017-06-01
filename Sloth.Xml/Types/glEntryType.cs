using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    public partial class glEntryType {
        
        private string fiscalYearField;
        
        private string chartField;
        
        private string accountField;
        
        private string subAccountField;
        
        private string objectField;
        
        private string subObjectField;
        
        private financialBalanceTypeCode balanceTypeField;
        
        private string objectTypeField;
        
        private universityFiscalPeriodCode fiscalPeriodField;
        
        private bool fiscalPeriodFieldSpecified;
        
        private financialDocumentTypeCode docTypeField;
        
        private string originCodeField;
        
        private string docNumField;
        
        private string sequenceNumField;
        
        private string descriptionField;
        
        private decimal amountField;
        
        private transactionDebitCreditCode debitCreditField;
        
        private System.DateTime transDateField;
        
        private string trackingNumberField;
        
        private string projectField;
        
        private string referenceIdField;
        
        private financialDocumentTypeCode refDocTypeField;
        
        private bool refDocTypeFieldSpecified;
        
        private string refOriginCodeField;
        
        private string refDocNumField;
        
        private System.DateTime reversalDateField;
        
        private bool reversalDateFieldSpecified;
        
        private transactionEncumbranceUpdateCode encumbCodeField;
        
        private bool encumbCodeFieldSpecified;
        
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
        public universityFiscalPeriodCode fiscalPeriod {
            get {
                return this.fiscalPeriodField;
            }
            set {
                this.fiscalPeriodField = value;
            }
        }
        
        /// <remarks/>
        [XmlIgnore()]
        public bool fiscalPeriodSpecified {
            get {
                return this.fiscalPeriodFieldSpecified;
            }
            set {
                this.fiscalPeriodFieldSpecified = value;
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
        public string docNum {
            get {
                return this.docNumField;
            }
            set {
                this.docNumField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="positiveInteger")]
        public string sequenceNum {
            get {
                return this.sequenceNumField;
            }
            set {
                this.sequenceNumField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
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
        public transactionDebitCreditCode debitCredit {
            get {
                return this.debitCreditField;
            }
            set {
                this.debitCreditField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="date")]
        public System.DateTime transDate {
            get {
                return this.transDateField;
            }
            set {
                this.transDateField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string trackingNumber {
            get {
                return this.trackingNumberField;
            }
            set {
                this.trackingNumberField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string project {
            get {
                return this.projectField;
            }
            set {
                this.projectField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string referenceId {
            get {
                return this.referenceIdField;
            }
            set {
                this.referenceIdField = value;
            }
        }
        
        /// <remarks/>
        public financialDocumentTypeCode refDocType {
            get {
                return this.refDocTypeField;
            }
            set {
                this.refDocTypeField = value;
            }
        }
        
        /// <remarks/>
        [XmlIgnore()]
        public bool refDocTypeSpecified {
            get {
                return this.refDocTypeFieldSpecified;
            }
            set {
                this.refDocTypeFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string refOriginCode {
            get {
                return this.refOriginCodeField;
            }
            set {
                this.refOriginCodeField = value;
            }
        }
        
        /// <remarks/>
        public string refDocNum {
            get {
                return this.refDocNumField;
            }
            set {
                this.refDocNumField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="date")]
        public System.DateTime reversalDate {
            get {
                return this.reversalDateField;
            }
            set {
                this.reversalDateField = value;
            }
        }
        
        /// <remarks/>
        [XmlIgnore()]
        public bool reversalDateSpecified {
            get {
                return this.reversalDateFieldSpecified;
            }
            set {
                this.reversalDateFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public transactionEncumbranceUpdateCode encumbCode {
            get {
                return this.encumbCodeField;
            }
            set {
                this.encumbCodeField = value;
            }
        }
        
        /// <remarks/>
        [XmlIgnore()]
        public bool encumbCodeSpecified {
            get {
                return this.encumbCodeFieldSpecified;
            }
            set {
                this.encumbCodeFieldSpecified = value;
            }
        }
    }
}
