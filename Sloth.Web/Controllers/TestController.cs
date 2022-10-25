using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Razor.Templating.Core;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Models.Emails;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Core.Views.Shared;
using Sloth.Web.Controllers;
using Sloth.Web.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Sloth.Web.Controllers
{
    [Authorize(Roles = Roles.SystemAdmin)]
    public class TestController : SuperController
    {
        private readonly INotificationService _notificationService;
        private readonly NotificationOptions _notificationOptions;

        public TestController(ApplicationUserManager userManager, SlothDbContext dbContext, INotificationService notificationService,
                              IOptions<NotificationOptions> notificationOptions) : base(userManager, dbContext)
        {
            _notificationService = notificationService;
            _notificationOptions = notificationOptions.Value;
        }

        public IActionResult ViewDefaultNotification()
        {
            var model = new DefaultNotificationModel
            {
                Subject = "Sloth Notification",
                MessageText = "This is a test notification",
                LinkBackButton = new EmailButtonModel
                {
                    Text = "Visit Sloth-Test",
                    UrlPath = ""
                }
            };

            ViewData["LinkBackBaseUrl"] = _notificationOptions.LinkBackBaseUrl;

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
                .WithEmails(emailAddress)
                .WithSubject("Test Email")
                .WithLinkBack("Visit Sloth"));

            return Ok();
        }


    }
}
