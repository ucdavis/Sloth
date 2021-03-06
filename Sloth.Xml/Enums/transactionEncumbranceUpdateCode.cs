using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("transactionEncumbranceUpdateCode", Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("encumbCode", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum TransactionEncumbranceUpdateCode {
        
        /// <remarks/>
        R,
        
        /// <remarks/>
        D,
        
        /// <remarks/>
        [XmlEnum(" ")]
        None,
    }
}
