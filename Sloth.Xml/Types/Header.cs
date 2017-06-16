using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlRoot("header", Namespace = KfsNamespace, IsNullable = false)]
    [XmlType("headerType", Namespace = KfsNamespace)]
    public class Header : KfsXmlElement
    {
        /// <summary>
        /// Chart of Accounts Code associate with Org Code.
        /// </summary>
        [MaxLength(2)]
        [Required]
        [XmlElement("chart", DataType = "normalizedString")]
        public string Chart { get; set; }

        /// <summary>
        /// Financial System Organization responsible for GL Feed.
        /// </summary>
        [MaxLength(4)]
        [Required]
        [XmlElement("orgCode", DataType = "normalizedString")]
        public string OrganizationCode { get; set; }

        /// <summary>
        /// Date the file was created from source system
        /// </summary>
        [Required]
        [XmlElement("batchDate", DataType = "date")]
        public DateTime BatchDate { get; set; }

        /// <summary>
        /// Source system batch sequence number
        /// </summary>
        [Range(1, 999999)]
        [Required]
        [XmlElement("batchSequenceNum", DataType = "int")]
        public int BatchSequenceNumber { get; set; }

        /// <summary>
        /// Campus User ID of main contact responsible for feed.
        /// </summary>
        [MinLength(2)]
        [MaxLength(8)]
        [Required]
        [XmlElement("contactUserId", DataType = "normalizedString")]
        public string ContactUserId { get; set; }

        /// <summary>
        /// Email address used to send information when errors occur or other events. The email should match the origination code email. A distribution list is highly recommended.
        /// </summary>
        [MaxLength(40)]
        [EmailAddress]
        [Required]
        [XmlElement("contactEmail", DataType = "normalizedString")]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Financial System only code. Value to be provided by Admin IT to the integration system.
        /// </summary>
        [MaxLength(2)]
        [Required]
        [XmlElement("campusCode")]
        public campusCode CampusCode { get; set; }

        /// <summary>
        /// Contact number for personnel/unit responsible for feed.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [RegularExpression("[0-9]*")]
        [Required]
        [XmlElement("contactPhoneNumber", DataType = "normalizedString")]
        public string ContactPhoneNumber { get; set; }

        /// <summary>
        /// Brief mailing address for Department responsible for feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        [XmlElement("mailingAddress", DataType = "normalizedString")]
        public string MailingAddress { get; set; }

        /// <summary>
        /// Department responsible for feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        [XmlElement("departmentName", DataType = "normalizedString")]
        public string DepartmentName { get; set; }
    }
}
