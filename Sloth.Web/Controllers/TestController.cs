using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using Sloth.Core;
using Sloth.Core.Models.Emails;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Core.Views.Shared;
using Sloth.Web.Controllers;
using Sloth.Web.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Harvest.Web.Controllers
{
    [Authorize(Roles = Roles.SystemAdmin)]
    public class TestController : SuperController
    {
        private readonly INotificationService _notificationService;

        public TestController(ApplicationUserManager userManager, SlothDbContext dbContext, INotificationService notificationService) : base(userManager, dbContext)
        {
            _notificationService = notificationService;
        }

        public IActionResult ViewDefaultNotification()
        {
            var model = new DefaultNotificationModel
            {
                Subject = "Transactions Notification",
                MessageText = "This is a test notification",
                LinkBackButton = new EmailButtonModel
                {
                    Text = "View Transactions",
                    Url = "https://www.google.com"
                }
            };

            return View("/Views/Emails/DefaultNotification.cshtml", model);
        }

        public async Task<IActionResult> TestEmail([FromQuery] string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return BadRequest("emailAddress is required");
            }

            await _notificationService.Notify(
                Notification.Message("This is a test email", emailAddress)
                .WithSubject("Test Email")
                .WithLinkBack("View Sloth", "https://sloth-test.ucdavis.edu"));

            return Ok();
        }


    }
}
