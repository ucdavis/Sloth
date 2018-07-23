using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Web.Models;

namespace Sloth.Web.Controllers
{
    public class HomeController : SuperController
    {
        public HomeController(UserManager<User> userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
