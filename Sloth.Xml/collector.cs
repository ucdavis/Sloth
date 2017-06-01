﻿using System.Xml.Serialization;

namespace Sloth.Xml {
    
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot(Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum campusCode {
        
        /// <remarks/>
        DV,
        
        /// <remarks/>
        DH,
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("balanceType", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum financialBalanceTypeCode {
        
        /// <remarks/>
        AC,
        
        /// <remarks/>
        CB,
        
        /// <remarks/>
        EX,
        
        /// <remarks/>
        IE,
        
        /// <remarks/>
        PE,
        
        /// <remarks/>
        BB,
        
        /// <remarks/>
        BI,
        
        /// <remarks/>
        FT,
        
        /// <remarks/>
        FI,
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("fiscalPeriod", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum universityFiscalPeriodCode {
        
        /// <remarks/>
        [XmlEnum("")]
        Item,
        
        /// <remarks/>
        [XmlEnum("01")]
        Item01,
        
        /// <remarks/>
        [XmlEnum("02")]
        Item02,
        
        /// <remarks/>
        [XmlEnum("03")]
        Item03,
        
        /// <remarks/>
        [XmlEnum("04")]
        Item04,
        
        /// <remarks/>
        [XmlEnum("05")]
        Item05,
        
        /// <remarks/>
        [XmlEnum("06")]
        Item06,
        
        /// <remarks/>
        [XmlEnum("07")]
        Item07,
        
        /// <remarks/>
        [XmlEnum("08")]
        Item08,
        
        /// <remarks/>
        [XmlEnum("09")]
        Item09,
        
        /// <remarks/>
        [XmlEnum("10")]
        Item10,
        
        /// <remarks/>
        [XmlEnum("11")]
        Item11,
        
        /// <remarks/>
        [XmlEnum("12")]
        Item12,
        
        /// <remarks/>
        [XmlEnum("13")]
        Item13,
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("docType", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum financialDocumentTypeCode {
        
        /// <remarks/>
        GLIB,
        
        /// <remarks/>
        GLJV,
        
        /// <remarks/>
        GLCC,
        
        /// <remarks/>
        GLJB,
        
        /// <remarks/>
        GLBB,
        
        /// <remarks/>
        GLCB,
        
        /// <remarks/>
        GLDE,
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("debitCredit", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum transactionDebitCreditCode {
        
        /// <remarks/>
        [XmlEnum(" ")]
        Item,
        
        /// <remarks/>
        D,
        
        /// <remarks/>
        C,
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("encumbCode", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum transactionEncumbranceUpdateCode {
        
        /// <remarks/>
        R,
        
        /// <remarks/>
        D,
        
        /// <remarks/>
        [XmlEnum(" ")]
        Item,
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("batch", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public partial class batchType {
        
        private headerType headerField;
        
        private glEntryType[] glEntryField;
        
        private detailType[] detailField;
        
        private trailerType trailerField;
        
        /// <remarks/>
        public headerType header {
            get {
                return this.headerField;
            }
            set {
                this.headerField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement("glEntry")]
        public glEntryType[] glEntry {
            get {
                return this.glEntryField;
            }
            set {
                this.glEntryField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement("detail")]
        public detailType[] detail {
            get {
                return this.detailField;
            }
            set {
                this.detailField = value;
            }
        }
        
        /// <remarks/>
        public trailerType trailer {
            get {
                return this.trailerField;
            }
            set {
                this.trailerField = value;
            }
        }
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    public partial class headerType {
        
        private string chartField;
        
        private string orgCodeField;
        
        private System.DateTime batchDateField;
        
        private string batchSequenceNumField;
        
        private string contactUserIdField;
        
        private string contactEmailField;
        
        private campusCode campusCodeField;
        
        private string contactPhoneNumberField;
        
        private string mailingAddressField;
        
        private string departmentNameField;
        
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
        public string orgCode {
            get {
                return this.orgCodeField;
            }
            set {
                this.orgCodeField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="date")]
        public System.DateTime batchDate {
            get {
                return this.batchDateField;
            }
            set {
                this.batchDateField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="positiveInteger")]
        public string batchSequenceNum {
            get {
                return this.batchSequenceNumField;
            }
            set {
                this.batchSequenceNumField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string contactUserId {
            get {
                return this.contactUserIdField;
            }
            set {
                this.contactUserIdField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string contactEmail {
            get {
                return this.contactEmailField;
            }
            set {
                this.contactEmailField = value;
            }
        }
        
        /// <remarks/>
        public campusCode campusCode {
            get {
                return this.campusCodeField;
            }
            set {
                this.campusCodeField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string contactPhoneNumber {
            get {
                return this.contactPhoneNumberField;
            }
            set {
                this.contactPhoneNumberField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string mailingAddress {
            get {
                return this.mailingAddressField;
            }
            set {
                this.mailingAddressField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string departmentName {
            get {
                return this.departmentNameField;
            }
            set {
                this.departmentNameField = value;
            }
        }
    }
    
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    public partial class trailerType {
        
        private string totalRecordsField;
        
        private decimal totalAmountField;
        
        /// <remarks/>
        [XmlElement(DataType="positiveInteger")]
        public string totalRecords {
            get {
                return this.totalRecordsField;
            }
            set {
                this.totalRecordsField = value;
            }
        }
        
        /// <remarks/>
        public decimal totalAmount {
            get {
                return this.totalAmountField;
            }
            set {
                this.totalAmountField = value;
            }
        }
    }
    
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
