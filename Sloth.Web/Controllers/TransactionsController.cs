using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;

namespace Sloth.Web.Controllers
{
    public class TransactionsController : SuperController
    {
        public TransactionsController(UserManager<User> userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var transactions = await DbContext.Transactions
                .AsNoTracking()
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> NeedApproval()
        {
            var transactions = await DbContext.Transactions
                .Where(t => t.Status == TransactionStatuses.PendingApproval)
                .AsNoTracking()
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> Details(string id)
        {
            var transaction = await DbContext.Transactions
                .Include(t => t.Scrubber)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Include(t => t.Transfers)
                .Include(t => t.ReversalOfTransaction)
                .FirstOrDefaultAsync(t => t.Id == id);

            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleTransaction(string id)
        {
            var transaction = await DbContext.Transactions
                .Include(t => t.Scrubber)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.Id == id);

            //TODO: return error messages and redirect
            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status != TransactionStatuses.PendingApproval)
            {
                return NotFound();
            }

            transaction.Status = TransactionStatuses.Scheduled;

            await DbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
