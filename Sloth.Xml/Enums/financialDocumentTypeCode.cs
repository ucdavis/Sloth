using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("financialDocumentTypeCode", Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("docType", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum FinancialDocumentTypeCode {
        
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
        ERROR,
    }
}
