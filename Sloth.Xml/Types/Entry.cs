using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlRoot("glEntry", Namespace = KfsNamespace, IsNullable = false)]
    [XmlType("glEntryType", Namespace = "http://www.kuali.org/kfs/gl/collector")]
    public class Entry : KfsXmlElement
    {
        /// <summary>
        /// Fiscal Year in which the feed will post to the General Ledger.
        /// It is highly recommended to include the fiscal period in all records.
        /// 
        /// Feed systems are generally only allowed to submit files for accounting periods 1-12.
        /// 
        /// If the fiscal year/period combination referenced on lines in the file is not open for posting,
        ///  then the entries will be defaulted to the current fiscal period as derived from the system’s date table
        /// </summary>
        [Range(1, 12)]
        [XmlElement("fiscalYear", DataType = "int")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Chart Code associated with transaction.
        /// </summary>
        [MaxLength(2)]
        [Required]
        [XmlElement("chart", DataType = "normalizedString")]
        public string Chart { get; set; }

        /// <summary>
        /// Account used in the general ledger to post transactions.
        /// Accounts are specific to a Chart Code.
        /// </summary>
        [MaxLength(7)]
        [RegularExpression("[A-Z0-9]*")]
        [Required]
        [XmlElement("account", DataType = "normalizedString")]
        public string Account { get; set; }

        /// <summary>
        /// Sub-Account is an optional accounting unit attribute.
        /// Chart Code and Account are part of Sub-Account key.
        /// </summary>
        [MaxLength(5)]
        [RegularExpression("[A-Z0-9]*")]
        [XmlElement("subAccount", DataType = "normalizedString")]
        public string SubAccount { get; set; }

        /// <summary>
        /// Object codes represent all income, expense, asset, liability and fund balance classification
        ///  that are assigned to transactions and help identify the nature of the transaction.
        /// Object Codes are specific to a Chart Code.
        /// </summary>
        [MaxLength(4)]
        [RegularExpression("[A-Z0-9]*")]
        [Required]
        [XmlElement("object", DataType = "normalizedString")]
        public string ObjectCode { get; set; }

        /// <summary>
        /// Sub-Object is an optional accounting unit attribute that allows finer 
        ///  distinctions within a particular object code on an account.
        /// Sub-Object codes are specific to a Chart Code, Account and Object Code combination.
        /// </summary>
        [MaxLength(3)]
        [RegularExpression("[A-Z0-9]*")]
        [XmlElement("subObject", DataType = "normalizedString")]
        public string SubObjectCode { get; set; }

        /// <summary>
        /// Defines the type of financial balances used in reporting and queries.
        /// Feed system will be authorized for specific Balance Types; typically only AC(actuals).
        /// </summary>
        [XmlElement("balanceType")]
        public financialBalanceTypeCode BalanceType { get; set; }

        /// <summary>
        /// Object Type defines the general use of an object code; such as income, asset, expense, or liability.
        /// Not a required field as the General Ledger will derives the value from the Object Code.
        /// It is recommended not to include these values.
        /// </summary>
        [MaxLength(2)]
        [XmlElement("objectType", DataType = "normalizedString")]
        public string ObjectType { get; set; }

        /// <summary>
        /// Fiscal Year in which the feed will post to the General Ledger.
        /// It is highly recommended to include the fiscal period in all records.
        /// 
        /// Feed systems are generally only allowed to submit files for accounting periods 1-12.
        /// 
        /// If the fiscal year/period combination referenced on lines in the file is not open for posting,
        ///  then the entries will be defaulted to the current fiscal period as derived from the system’s date table
        /// </summary>
        [XmlElement("fiscalPeriod")]
        public universityFiscalPeriodCode FiscalPeriod { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool FiscalPeriodSpecified { get; set; }

        /// <summary>
        /// Financial System document type associated with the feed.
        /// Feed systems will be authorized to use a specific value based on transactions.
        /// </summary>
        [Required]
        [XmlElement("docType")]
        public financialDocumentTypeCode DocType { get; set; }

        /// <summary>
        /// Unique feed origination identifier given to the Feed System.
        /// The origination code is validated in during file receipt and in the processing.
        /// </summary>
        [MinLength(2)]
        [MaxLength(2)]
        [Required]
        [XmlElement("originCode", DataType = "normalizedString")]
        public string OriginCode { get; set; }

        /// <summary>
        /// Unique identifier for a set of related transactions per origination code.
        /// A file can have multiple document numbers but the file must balance by document number(aka net zero) and by total amount.Debits = Credits
        /// Once a document number posts to the general ledger then it cannot be used again.
        /// </summary>
        [MinLength(1)]
        [MaxLength(14)]
        [RegularExpression("[A-Z0-9]*")]
        [Required]
        [XmlElement("docNum")]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// A sequential number for a transaction in document number set.
        /// The system will auto-assign if not provided.
        /// </summary>
        [Range(1, 99999)]
        [XmlElement("sequenceNum", DataType = "int")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// A brief description of the specific transaction. Displays in reporting.
        /// PCI, HIPPA, FERPA and PII information is prohibited.
        /// </summary>
        [MaxLength(40)]
        [Required]
        [XmlElement("description", DataType = "normalizedString")]
        public string Description { get; set; }

        /// <summary>
        /// Dollar amount associated with the transaction
        /// </summary>
        [Range(typeof(decimal), "0.01", "1000000000")]
        [Required]
        [XmlElement("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Debit or Credit Code associated with the transaction.
        /// </summary>
        [Required]
        [XmlElement("debitCredit")]
        public transactionDebitCreditCode DebitCredit { get; set; }

        /// <summary>
        /// Date the transaction occurred.
        /// </summary>
        [Required]
        [XmlElement("transDate", DataType = "date")]
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Primarily used in Decision Support reporting for additional transaction identification.
        /// Equivalent to the KFS Organization Document Number.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [XmlElement("trackingNumber", DataType = "normalizedString")]
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Project is an optional accounting unit attribute that allows assignment of an identifier
        ///  to particular transactions that might span multiple accounts.
        /// Because Project Code is not specific to an account it can be used to track project activity
        ///  that is shared across multiple accounts within an organization or even across multiple organizations.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [XmlElement("project", DataType = "normalizedString")]
        public string Project { get; set; }

        /// <summary>
        /// Optional field for organizational reference purposes.
        /// </summary>
        [MinLength(1)]
        [MaxLength(8)]
        [XmlElement("referenceId", DataType = "normalizedString")]
        public string ReferenceId { get; set; }

        /// <summary>
        /// Reference Document Type associated with transaction.
        /// Usage is not recommended unless reversing transactions from prior feed.
        /// </summary>
        [XmlElement("refDocType")]
        public financialDocumentTypeCode RefDocType { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool RefDocTypeSpecified { get; set; }

        /// <summary>
        /// Reference Origination Code.
        /// Usage is not recommended unless reversing transactions from prior feed.
        /// </summary>
        [MinLength(2)]
        [MaxLength(2)]
        [XmlElement("refOriginCode", DataType = "normalizedString")]
        public string RefOriginCode { get; set; }

        /// <summary>
        /// Reference Document Number.
        /// Usage is not recommended unless reversing transactions from prior feed.
        /// </summary>
        [MinLength(1)]
        [MaxLength(14)]
        [RegularExpression("[A-Z0-9]*")]
        [XmlElement("refDocNum")]
        public string RefDocNum { get; set; }

        /// <summary>
        /// For Accounting & Financial Services use only.
        /// </summary>
        [XmlElement("reveralDate", DataType = "date")]
        public DateTime ReversalDate { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool ReversalDateSpecified { get; set; }

        /// <summary>
        /// For Accounting & Financial Services use only.
        /// </summary>
        [XmlElement("encumbCode")]
        public transactionEncumbranceUpdateCode EncumbCode { get; set; }

        /// <remarks/>
        [XmlIgnore()]
        public bool EncumbCodeSpecified { get; set; }
    }
}
