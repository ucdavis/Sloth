using System;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Web.Models.TeamViewModels
{
    public class TeamViewModel
    {
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Campus User ID of main contact responsible for KFS feed.
        /// </summary>
        [MinLength(2)]
        [MaxLength(8)]
        [Required]
        [Display(Name = "KFS Contact Kerberos Id")]
        public string KfsContactUserId { get; set; }

        /// <summary>
        /// Email address used to send information when errors occur or other events with the KFS feed.
        /// The email should match the origination code email.
        /// A distribution list is highly recommended.
        /// </summary>
        [MaxLength(40)]
        [EmailAddress]
        [Required]
        [Display(Name = "KFS Contact Email")]
        public string KfsContactEmail { get; set; }

        /// <summary>
        /// Contact number for personnel/unit responsible for KFS feed.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [RegularExpression("[0-9]*")]
        [Required]
        [Display(Name = "KFS Contact Phone Number")]
        public string KfsContactPhoneNumber { get; set; }

        /// <summary>
        /// Brief mailing address for Department responsible for KFS feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        [Display(Name = "KFS Contact Mailing Address")]
        public string KfsContactMailingAddress { get; set; }

        /// <summary>
        /// Department responsible for KFS feed.
        /// </summary>
        [MaxLength(30)]
        [Required]
        [Display(Name = "KFS Contact Department")]
        public string KfsContactDepartmentName { get; set; }
    }
}
