using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using Sloth.Core;
using Sloth.Core.Models.Emails;
using Sloth.Core.Resources;
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
        public TestController(ApplicationUserManager userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        public async Task<IActionResult> GenerateTransactionsNotificationTemplate()
        {
            var model = new TransactionsNotificationModel();

            var results = await RazorTemplateEngine.RenderAsync("/Views/Emails/TransactionsNotification_mjml.cshtml", model);

            return Content(results);
        }

        public IActionResult ViewTransactionsNotification()
        {
            var model = new TransactionsNotificationModel
            {
                Title = "Transactions Notification",
                NotificationText = "This is a test notification",
                ButtonText1 = "View Transactions",
                ButtonUrl1 = "https://www.google.com"
            };

            return View("/Views/Emails/TransactionsNotification.cshtml", model);
        }

        // public async Task<IActionResult> TestEmail()
        // {
        //     var user = await _userService.GetCurrentUser();

        //     var model = await _dbContext.Projects.Where(a => a.IsActive && a.Name != null && a.End <= DateTime.UtcNow.AddYears(1))
        //         .OrderBy(a => a.End).Take(5).Select(s => new ExpiringProjectsModel
        //         {
        //             EndDate = s.End.ToShortDateString(), Name = s.Name,
        //             ProjectUrl = $"https://harvest.caes.ucdavis.edu/Project/Details/{s.Id}"
        //         }).ToArrayAsync();



        //     var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ExpiringProjects.cshtml", model);
        //     await _notificationService.SendNotification(new string[] { user.Email }, null, emailBody, "EXPIRE", "EXPIRE");

        //     return Content("Done. Maybe. Well, possibly. If you don't get it, check the settings.");
        // }


    }
}
