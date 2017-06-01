using System.Xml.Serialization;

namespace Sloth.Xml
{
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
}
