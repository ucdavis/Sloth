using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;

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
    }
}
