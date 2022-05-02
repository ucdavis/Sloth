using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Sloth.Core.Models
{
    public class Transfer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public Transaction Transaction { get; set; }

        /// <summary>
        /// Dollar amount associated with the transaction
        /// </summary>
        [Range(typeof(decimal), "0.01", "1000000000")]
        [Required]
        public decimal Amount { get; set; }


        /// <summary>
        /// Required: Entity to which to charge a transaction.
        /// ErpEntityCode
        /// All values are exactly 4 characters matching the regex pattern: [0-9]{3}[0-9AB]
        /// </summary>
        [MaxLength(4)]
        [RegularExpression("[0-9]{3}[0-9AB]")]
        //[Required] We can't really have this as required if it is a POET instead of GL
        public string AeEntity { get; set; }

        /// <summary>
        /// Required: Funding source to which to charge a transaction.
        /// ErpFundCode
        /// All values are exactly 5 characters matching the regex pattern: [0-9A-Z][0-9]{3}[0-9A-Z]
        /// </summary>
        //[Required] We can't really have this as required if it is a POET instead of GL
        [MaxLength(5)]
        [RegularExpression("[0-9A-Z][0-9]{3}[0-9A-Z]")]
        public string AeFund { get; set; }

        /// <summary>
        /// Required: Financial department to which to charge a transaction.
        /// ErpDepartmentCode
        /// All values are exactly 7 characters matching the regex pattern: [0-9P][0-9]{5}[0-9A-F]
        /// </summary>
        //[Required] We can't really have this as required if it is a POET instead of GL
        [MaxLength(7)]
        [RegularExpression("[0-9P][0-9]{5}[0-9A-F]")]
        public string AeDepartment { get; set; }

        /// <summary>
        /// Required: Nature of the transaction, expense, income, liability, etc...
        /// ErpAccountCode
        /// All values are exactly 6 characters matching the regex pattern: [0-9]{5}[0-9A-EX]
        /// </summary>
        //[Required] We can't really have this as required if it is a POET instead of GL
        [MaxLength(6)]
        [RegularExpression("[0-9]{5}[0-9A-EX]")]
        public string AeAccount { get; set; }

        /// <summary>
        /// Required for Expenses: Functional purpose of the expense.
        /// ErpPurposeCode
        /// All values are exactly 2 characters matching the regex pattern: [0-9][0-9A-Z]
        /// </summary>
        //[Required] We can't really have this as required if it is a POET instead of GL
        [MaxLength(2)]
        [RegularExpression("[0-9][0-9A-Z]")]
        public string AePurpose { get; set; }

        /// <summary>
        /// Optional:
        /// ErpProjectCode
        /// All values are exactly 10 characters matching the regex pattern: [0-9A-Z]{10}
        /// </summary>
        [MaxLength(10)]
        [RegularExpression("[0-9A-Z]{10}")]
        public string AeProject { get; set; }

        /// <summary>
        /// Optional
        /// ErpProgramCode
        /// All values are exactly 3 characters matching the regex pattern: [0-9A-Z]{3}
        /// </summary>
        [MaxLength(3)]
        [RegularExpression("[0-9A-Z]{3}")]
        public string AeProgram { get; set; }

        /// <summary>
        /// Optional
        /// ErpActivityCode
        /// All values are exactly 6 characters matching the regex pattern: [0-9X]{5}[0-9AB]
        /// </summary>
        [MaxLength(6)]
        [RegularExpression("[0-9X]{5}[0-9AB]")]
        public string AeActivity { get; set; }

        //AeFlex1 and AeFlex2 : Unused: For future UCOP Reporting Requirements. Always 000000

        /// <summary>
        /// Chart Code associated with transaction.
        /// </summary>
        [MaxLength(1)]
        [Required]
        public string Chart { get; set; }

        /// <summary>
        /// Account used in the general ledger to post transactions.
        /// Accounts are specific to a Chart Code.
        /// </summary>
        [MaxLength(7)]
        [RegularExpression("[A-Z0-9]*")]
        [Required]
        public string Account { get; set; }

        /// <summary>
        /// Sub-Account is an optional accounting unit attribute.
        /// Chart Code and Account are part of Sub-Account key.
        /// </summary>
        [MaxLength(5)]
        [RegularExpression("[A-Z0-9]*")]
        public string SubAccount { get; set; }

        /// <summary>
        /// Object codes represent all income, expense, asset, liability and fund balance classification
        ///  that are assigned to transactions and help identify the nature of the transaction.
        /// Object Codes are specific to a Chart Code.
        /// </summary>
        [MaxLength(4)]
        [RegularExpression("[A-Z0-9]*")]
        [Required]
        [Display(Name = "Object Code")]
        public string ObjectCode { get; set; }

        /// <summary>
        /// Sub-Object is an optional accounting unit attribute that allows finer 
        ///  distinctions within a particular object code on an account.
        /// Sub-Object codes are specific to a Chart Code, Account and Object Code combination.
        /// </summary>
        [MaxLength(3)]
        [RegularExpression("[A-Z0-9]*")]
        public string SubObjectCode { get; set; }

        /// <summary>
        /// Object Type defines the general use of an object code; such as income, asset, expense, or liability.
        /// Not a required field as the General Ledger will derives the value from the Object Code.
        /// It is recommended not to include these values.
        /// </summary>
        [MaxLength(2)]
        public string ObjectType { get; set; }

        /// <summary>
        /// A sequential number for a transaction in document number set.
        /// The system will auto-assign if not provided.
        /// </summary>
        [Range(1, 99999)]
        public int? SequenceNumber { get; set; }

        /// <summary>
        /// A brief description of the specific transaction. Displays in reporting.
        /// PCI, HIPPA, FERPA and PII information is prohibited.
        /// </summary>
        [MaxLength(40)]
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Debit or Credit Code associated with the transaction.
        /// </summary>
        [Required]
        public CreditDebit Direction { get; set; }

        /// <summary>
        /// Fiscal Year in which the feed will post.
        /// It is recommended to include the fiscal year in all records.
        /// Entries will default during general ledger processing based on run date.
        /// </summary>
        [Range(2017, 2099)]
        [Display(Name = "Fiscal Year")]
        public int? FiscalYear { get; set; }

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
        [Display(Name = "Fiscal Period")]
        public int? FiscalPeriod { get; set; }

        /// <summary>
        /// Project is an optional accounting unit attribute that allows assignment of an identifier
        ///  to particular transactions that might span multiple accounts.
        /// Because Project Code is not specific to an account it can be used to track project activity
        ///  that is shared across multiple accounts within an organization or even across multiple organizations.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        public string Project { get; set; }

        /// <summary>
        /// Optional field for organizational reference purposes.
        /// </summary>
        [MinLength(1)]
        [MaxLength(8)]
        public string ReferenceId { get; set; }

        #region Helpers
        public string FullAccountToString()
        {
            var result = Chart + "-" + Account;
            if (!string.IsNullOrEmpty(SubAccount))
            {
                result += "-" + SubAccount;
            }
            return result;
        }

        public string FullObjectToString()
        {
            var result = ObjectCode;
            if (!string.IsNullOrEmpty(SubObjectCode))
            {
                result += "-" + SubObjectCode;
            }
            return result;
        }
        #endregion

        public enum CreditDebit
        {
            /// <summary>
            /// Add money to an account
            /// </summary>
            Credit,

            /// <summary>
            /// Remove money from an account
            /// </summary>
            Debit
        }

        public static string GetDirectionBadgeClass(CreditDebit direction)
        {
            switch (direction)
            {
                case CreditDebit.Credit:
                    return "badge-success";

                case CreditDebit.Debit:
                    return "badge-primary";

                default:
                    return "badge-secondary";
            }
        }
    }
}
