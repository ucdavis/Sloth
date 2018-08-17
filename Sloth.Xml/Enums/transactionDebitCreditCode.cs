using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("transactionDebitCreditCode", Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("debitCredit", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum TransactionDebitCreditCode {
        
        /// <remarks/>
        [XmlEnum(" ")]
        None,
        
        /// <remarks>Debit</remarks>
        [XmlEnum("D")]
        Debit,

        /// <remarks>Credit</remarks>
        [XmlEnum("C")]
        Credit,
    }
}
