using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlType(Namespace="http://www.kuali.org/kfs/gl/collector")]
    public partial class headerType {
        
        private string chartField;
        
        private string orgCodeField;
        
        private System.DateTime batchDateField;
        
        private string batchSequenceNumField;
        
        private string contactUserIdField;
        
        private string contactEmailField;
        
        private campusCode campusCodeField;
        
        private string contactPhoneNumberField;
        
        private string mailingAddressField;
        
        private string departmentNameField;
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string chart {
            get {
                return this.chartField;
            }
            set {
                this.chartField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string orgCode {
            get {
                return this.orgCodeField;
            }
            set {
                this.orgCodeField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="date")]
        public System.DateTime batchDate {
            get {
                return this.batchDateField;
            }
            set {
                this.batchDateField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="positiveInteger")]
        public string batchSequenceNum {
            get {
                return this.batchSequenceNumField;
            }
            set {
                this.batchSequenceNumField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string contactUserId {
            get {
                return this.contactUserIdField;
            }
            set {
                this.contactUserIdField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string contactEmail {
            get {
                return this.contactEmailField;
            }
            set {
                this.contactEmailField = value;
            }
        }
        
        /// <remarks/>
        public campusCode campusCode {
            get {
                return this.campusCodeField;
            }
            set {
                this.campusCodeField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string contactPhoneNumber {
            get {
                return this.contactPhoneNumberField;
            }
            set {
                this.contactPhoneNumberField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string mailingAddress {
            get {
                return this.mailingAddressField;
            }
            set {
                this.mailingAddressField = value;
            }
        }
        
        /// <remarks/>
        [XmlElement(DataType="normalizedString")]
        public string departmentName {
            get {
                return this.departmentNameField;
            }
            set {
                this.departmentNameField = value;
            }
        }
    }
}
