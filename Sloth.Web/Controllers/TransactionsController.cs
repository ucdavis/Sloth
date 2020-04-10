using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Identity;
using Sloth.Web.Models.TransactionViewModels;

namespace Sloth.Web.Controllers
{
    public class TransactionsController : SuperController
    {
        public TransactionsController(ApplicationUserManager userManager, SlothDbContext dbContext) : base(userManager, dbContext)
        {
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            return await Filtered(DateTime.Now.AddMonths(-1), DateTime.Now);
        }

        public async Task<IActionResult> Filtered(DateTime from, DateTime to, string merchantId = "", string trackingNum = "")
        {
            var fromUtc = from.ToUniversalTime().Date;
            var throughUtc = to.ToUniversalTime().AddDays(1).Date;
            var redirectNeeded = false;

            if (fromUtc > DateTime.UtcNow || fromUtc < DateTime.UtcNow.AddYears(-100)) 
            {
                // invalid, so default to filtering from one month ago
                from = DateTime.Now.AddMonths((-1)).Date;
                fromUtc = from.ToUniversalTime();
                redirectNeeded = true;
            }

            if (fromUtc >= throughUtc)
            {
                // invalid, so default to filtering through one month after fromUtc
                throughUtc = fromUtc.AddMonths(1).AddDays(1).Date;
                to = throughUtc.AddDays(-1).ToLocalTime();
                redirectNeeded = true;
            }

            if (redirectNeeded)
            {
                return new RedirectResult($"/{TeamSlug}/transactions/filtered/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}");
            }

            var teamMerchants = await DbContext.Integrations
                .Where(i => i.Team.Slug == TeamSlug)
                .OrderBy(i => i.MerchantId)
                .Select(i => i.MerchantId)
                .ToListAsync();

            var query = DbContext.Transactions
                .Include(t => t.Transfers)
                .Where(t => t.Source.Team.Slug == TeamSlug && t.TransactionDate >= fromUtc &&
                            t.TransactionDate < throughUtc);

            if (!string.IsNullOrWhiteSpace(merchantId) && teamMerchants.Contains(merchantId))
            {
                query = query
                    .Where(t => t.Source.Team.Integrations.Any(
                        i => i.MerchantId == merchantId
                        && i.Source == t.Source));
            }

            if (!string.IsNullOrWhiteSpace(trackingNum))
            {
                query = query
                    .Where(t => t.KfsTrackingNumber == trackingNum
                                || t.MerchantTrackingNumber == trackingNum
                                || t.ProcessorTrackingNumber == trackingNum);
            }

            var transactions = await query
                .AsNoTracking()
                .ToListAsync();

            var result = new TransactionsReturnedViewModel()
            {
                TrackingNumber = trackingNum ?? "",
                TeamMerchantIds = teamMerchants,
                SelectedMerchantId = merchantId ?? "",
                From = from.Date,
                To = to.Date,
                Transactions = transactions
            };

            return View("Index", result);
        }

        public async Task<IActionResult> Search(string trackingNum = "")
        {
            var query =
                from t in DbContext.Transactions
                join t2 in DbContext.Transactions.Where(t => t.ProcessorTrackingNumber == trackingNum)
                    on t.MerchantTrackingNumber equals t2.MerchantTrackingNumber into joined
                from j in joined.DefaultIfEmpty()
                where j != null
                      || t.KfsTrackingNumber == trackingNum
                      || t.MerchantTrackingNumber == trackingNum
                select t;

            var transactions = await query
                .Include(t => t.Transfers)
                .AsNoTracking()
                .ToListAsync();

            var result = new TransactionsSearchViewModel()
            {
                TrackingNumber = trackingNum ?? "",
                Transactions = transactions
            };

            return View("Search", result);
        }

        public async Task<IActionResult> NeedApproval()
        {
            var transactions = await DbContext.Transactions
                .Include(t => t.Transfers)
                .Where(t => t.Source.Team.Slug == TeamSlug)
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
                .Where(t => t.Source.Team.Slug == TeamSlug)
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
                .Include(t => t.ReversalTransaction)
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

            // save transaction to establish id
            await DbContext.Transactions.AddAsync(reversal); 
            await DbContext.SaveChangesAsync();

            // save relationship
            transaction.AddReversalTransaction(reversal);
            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = reversal.Id });
        }
    }
}
