using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType("universityFisccalPeriodCode", Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("fiscalPeriod", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public enum UniversityFiscalPeriodCode {
        
        /// <remarks/>
        [XmlEnum("")]
        None,
        
        /// <remarks/>
        [XmlEnum("01")]
        Period01,
        
        /// <remarks/>
        [XmlEnum("02")]
        Period02,
        
        /// <remarks/>
        [XmlEnum("03")]
        Period03,
        
        /// <remarks/>
        [XmlEnum("04")]
        Period04,
        
        /// <remarks/>
        [XmlEnum("05")]
        Period05,
        
        /// <remarks/>
        [XmlEnum("06")]
        Period06,
        
        /// <remarks/>
        [XmlEnum("07")]
        Period07,
        
        /// <remarks/>
        [XmlEnum("08")]
        Period08,
        
        /// <remarks/>
        [XmlEnum("09")]
        Period09,
        
        /// <remarks/>
        [XmlEnum("10")]
        Period10,
        
        /// <remarks/>
        [XmlEnum("11")]
        Period11,
        
        /// <remarks/>
        [XmlEnum("12")]
        Period12,
        
        /// <remarks/>
        [XmlEnum("13")]
        Period13,
    }
}
