using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Resources;

namespace Sloth.Web.Controllers
{
    public class TransactionsController : SuperController
    {
        private readonly SlothDbContext _context;

        public TransactionsController(SlothDbContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .AsNoTracking()
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> Details(string id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Scrubber)
                .Include(t => t.Transfers)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleTransaction(string id)
        {
            var transaction = await _context.Transactions
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

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
