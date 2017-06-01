using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    [XmlRoot("batch", Namespace="http://www.kuali.org/kfs/gl/collector", IsNullable=false)]
    public partial class batchType {
        
        private headerType headerField;
        
        private glEntryType[] glEntryField;
        
        private detailType[] detailField;
        
        private trailerType trailerField;
        
        /// <remarks/>
        public headerType header {
            get {
                return this.headerField;
            }
            set {
                this.headerField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement("glEntry")]
        public glEntryType[] glEntry {
            get {
                return this.glEntryField;
            }
            set {
                this.glEntryField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement("detail")]
        public detailType[] detail {
            get {
                return this.detailField;
            }
            set {
                this.detailField = value;
            }
        }
        
        /// <remarks/>
        public trailerType trailer {
            get {
                return this.trailerField;
            }
            set {
                this.trailerField = value;
            }
        }
    }
}
