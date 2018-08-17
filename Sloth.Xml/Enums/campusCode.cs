using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("campusCode", Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot(Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum CampusCode {
        
        /// <remarks>Davis</remarks>
        [XmlEnum("DV")]
        DV,

        /// <remarks>Davis Health</remarks>
        [XmlEnum("DH")]
        DH,
    }
}
