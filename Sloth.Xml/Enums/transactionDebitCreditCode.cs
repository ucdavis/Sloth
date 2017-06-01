using System.Xml.Serialization;

namespace Sloth.Xml
{
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
}
