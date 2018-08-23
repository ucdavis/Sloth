using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Models;

namespace Sloth.Web.Controllers
{
    public class HomeController : SuperController
    {
        public HomeController(UserManager<User> userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        public async Task<IActionResult> Index()
        {
            var needApproval = await DbContext.Transactions
                .Include(t => t.Transfers)
                .Where(t => t.Status == TransactionStatuses.PendingApproval)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.NeedApproval = needApproval;


            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
