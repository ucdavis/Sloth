using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("balanceType", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum financialBalanceTypeCode {
        
        /// <remarks/>
        AC,
        
        /// <remarks/>
        CB,
        
        /// <remarks/>
        EX,
        
        /// <remarks/>
        IE,
        
        /// <remarks/>
        PE,
        
        /// <remarks/>
        BB,
        
        /// <remarks/>
        BI,
        
        /// <remarks/>
        FT,
        
        /// <remarks/>
        FI,
    }
}
