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
                .Include(t => t.Transfers)
                .AsNoTracking()
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> NeedApproval()
        {
            var transactions = await DbContext.Transactions
                .Include(t => t.Transfers)
                .Where(t => t.Status == TransactionStatuses.PendingApproval)
                .AsNoTracking()
                .ToListAsync();

            return View(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> ApprovalAll()
        {
            // fetch transactions
            var transactions = await DbContext.Transactions
                .Include(t => t.Transfers)
                .Where(t => t.Status == TransactionStatuses.PendingApproval)
                .ToListAsync();

            // update status
            transactions.ForEach(t => t.Status = TransactionStatuses.Scheduled);

            // save to db
            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

        [HttpPost]
        public async Task<IActionResult> CreateReversal(string id)
        {
            var transaction = await DbContext.Transactions
                .Include(t => t.Scrubber)
                .Include(t => t.Source)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            // you can only reverse a completed transaction
            if (transaction.Status != TransactionStatuses.Completed)
            {
                return BadRequest("Cannot reverse incomplete transaction.");
            }

            // you can only reverse a transaction once
            if (transaction.HasReversal)
            {
                return BadRequest("Cannot reverse transaction again");
            }

            var user = await UserManager.GetUserAsync(User);

            // create new transaction
            var reversal = new Transaction
            {
                Source                  = transaction.Source,
                Creator                 = user,
                TransactionDate         = DateTime.UtcNow,
                Status                  = TransactionStatuses.Scheduled,
                KfsTrackingNumber       = transaction.KfsTrackingNumber,
                MerchantTrackingNumber  = transaction.MerchantTrackingNumber,
                MerchantTrackingUrl     = transaction.MerchantTrackingUrl,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
            };

            // add reversal transfers
            foreach (var transfer in transaction.Transfers)
            {
                // same info, except reverse direction
                var direction = transfer.Direction == Transfer.CreditDebit.Credit
                    ? Transfer.CreditDebit.Debit
                    : Transfer.CreditDebit.Credit;

                reversal.Transfers.Add(new Transfer
                {
                    Amount         = transfer.Amount,
                    Account        = transfer.Account,
                    Chart          = transfer.Chart,
                    Description    = transfer.Description,
                    Direction      = direction,
                    FiscalPeriod   = transfer.FiscalPeriod,
                    FiscalYear     = transfer.FiscalYear,
                    ObjectCode     = transfer.ObjectCode,
                    ObjectType     = transfer.ObjectType,
                    Project        = transfer.Project,
                    ReferenceId    = transfer.ReferenceId,
                    SequenceNumber = transfer.SequenceNumber,
                    SubAccount     = transfer.SubAccount,
                    SubObjectCode  = transfer.SubObjectCode,
                });
            }

            // setup relationship
            transaction.AddReversalTransaction(reversal);
            await DbContext.Transactions.AddAsync(reversal); 
            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = reversal.Id });
        }
    }
}
