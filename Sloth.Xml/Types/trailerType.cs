using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    public partial class trailerType {
        
        private string totalRecordsField;
        
        private decimal totalAmountField;
        
        /// <remarks/>
        [XmlElement(DataType="positiveInteger")]
        public string totalRecords {
            get {
                return this.totalRecordsField;
            }
            set {
                this.totalRecordsField = value;
            }
        }
        
        /// <remarks/>
        public decimal totalAmount {
            get {
                return this.totalAmountField;
            }
            set {
                this.totalAmountField = value;
            }
        }
    }
}
