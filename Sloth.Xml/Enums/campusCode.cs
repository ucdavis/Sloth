using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot(Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum campusCode {
        
        /// <remarks/>
        DV,
        
        /// <remarks/>
        DH,
    }
}
