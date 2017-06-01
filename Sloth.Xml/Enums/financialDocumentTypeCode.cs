﻿using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("docType", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum financialDocumentTypeCode {
        
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
    }
}