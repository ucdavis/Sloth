using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// KFS: Chart Code associated with transaction.
        /// </summary>
        [MaxLength(1)]
        public string Chart { get; set; }

        /// <summary>
        /// KFS: Account used in the general ledger to post transactions.
        /// Accounts are specific to a Chart Code.
        /// </summary>
        [MaxLength(7)]
        [RegularExpression("[A-Z0-9]*")]
        public string Account { get; set; }

        /// <summary>
        /// KFS: Sub-Account is an optional accounting unit attribute.
        /// Chart Code and Account are part of Sub-Account key.
        /// </summary>
        [MaxLength(5)]
        [RegularExpression("[A-Z0-9]*")]
        public string SubAccount { get; set; }

        /// <summary>
        /// KFS: Object codes represent all income, expense, asset, liability and fund balance classification
        ///  that are assigned to transactions and help identify the nature of the transaction.
        /// Object Codes are specific to a Chart Code.
        /// </summary>
        [MaxLength(4)]
        [RegularExpression("[A-Z0-9]*")]
        [Display(Name = "Object Code")]
        public string ObjectCode { get; set; }

        /// <summary>
        /// KFS: Sub-Object is an optional accounting unit attribute that allows finer
        ///  distinctions within a particular object code on an account.
        /// Sub-Object codes are specific to a Chart Code, Account and Object Code combination.
        /// </summary>
        [MaxLength(3)]
        [RegularExpression("[A-Z0-9]*")]
        public string SubObjectCode { get; set; }

        /// <summary>
        /// KFS: Object Type defines the general use of an object code; such as income, asset, expense, or liability.
        /// Not a required field as the General Ledger will derives the value from the Object Code.
        /// It is recommended not to include these values.
        /// </summary>
        [MaxLength(2)]
        public string ObjectType { get; set; }

        /// <summary>
        /// Aggie Enterprise: Full COA string (GL or PPM)
        /// To use Aggie Enterprise, fill out this field instead of Chat, Account, etc.
        /// </summary>
        [MaxLength(128)]
        [Display(Name = "Aggie Enterprise COA String")]
        public string FinancialSegmentString { get; set; }

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
        /// KFS: Fiscal Year in which the feed will post.
        /// It is recommended to include the fiscal year in all records.
        /// Entries will default during general ledger processing based on run date.
        /// </summary>
        [Range(2017, 2099)]
        [Display(Name = "Fiscal Year")]
        public int? FiscalYear { get; set; }

        /// <summary>
        /// KFS: Fiscal Year in which the feed will post to the General Ledger.
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
        /// Aggie Enterprise: Optional: The accounting date of the transfer. Will be defaulted to today's date if not provided.
        /// </summary>
        [Display(Name = "Accounting Date")]
        public DateTime? AccountingDate { get; set; }

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
