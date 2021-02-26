using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Models.TransactionViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamApprover)]
    public class TransactionsController : SuperController
    {
        private readonly IWebHookService WebHookService;

        public TransactionsController(ApplicationUserManager userManager, SlothDbContext dbContext, IWebHookService webHookService) : base(userManager, dbContext)
        {
            WebHookService = webHookService;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index(TransactionsFilterModel filter = null)
        {
            if (filter == null)
                filter = new TransactionsFilterModel();

            SanitizeTransactionsFilter(filter);

            IQueryable<Transaction> query;

            List<string> teamMerchantIds = null;

            var result = new TransactionsIndexViewModel()
            {
                Filter = filter,
            };

            if (!string.IsNullOrWhiteSpace(filter.TrackingNum))
            {
                query = DbContext.Transactions
                    .Where(t => t.Source.Team.Slug == TeamSlug
                                && (t.ProcessorTrackingNumber == filter.TrackingNum
                                    || t.KfsTrackingNumber == filter.TrackingNum
                                    || t.MerchantTrackingNumber == filter.TrackingNum));

            }
            else
            {
                var fromUtc = (filter.From ?? DateTime.Now.AddMonths(-1)).ToUniversalTime().Date;
                var throughUtc = (filter.To ?? DateTime.Now.AddDays(1)).ToUniversalTime().Date;

                teamMerchantIds = await DbContext.Integrations
                    .Where(i => i.Team.Slug == TeamSlug)
                    .OrderBy(i => i.MerchantId)
                    .Select(i => i.MerchantId)
                    .ToListAsync();

                query = DbContext.Transactions
                    .Where(t => t.Source.Team.Slug == TeamSlug && t.TransactionDate >= fromUtc &&
                                t.TransactionDate < throughUtc);

                if (!string.IsNullOrWhiteSpace(filter.SelectedMerchantId) && teamMerchantIds.Contains(filter.SelectedMerchantId))
                {
                    query = query
                        .Where(t => t.Source.Team.Integrations.Any(
                            i => i.MerchantId == filter.SelectedMerchantId
                                 && i.Source == t.Source));
                }

                result.TeamMerchantIds =
                    teamMerchantIds.Select(i => new SelectListItem() {Value = i, Text = i}).ToList();
            }

            result.TransactionsTable = new TransactionsTableViewModel()
            {
                Transactions = await query
                    .Include(t => t.Transfers)
                    .AsNoTracking()
                    .ToListAsync(),
                HasWebhooks = await DbContext.WebHooks
                    .AnyAsync(w => w.Team.Slug == TeamSlug)
            };

            return View("Index", result);
        }

        public async Task<IActionResult> NeedApproval()
        {
            var transactionsTable = new TransactionsTableViewModel()
            {
                Transactions = await DbContext.Transactions
                    .Include(t => t.Transfers)
                    .Where(t => t.Source.Team.Slug == TeamSlug)
                    .Where(t => t.Status == TransactionStatuses.PendingApproval)
                    .AsNoTracking()
                    .ToListAsync(),
                HasWebhooks = await DbContext.WebHooks
                    .AnyAsync(w => w.Team.Slug == TeamSlug)
            };

            return View(transactionsTable);
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
            transactions.ForEach(t => t.SetStatus(TransactionStatuses.Scheduled));

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

            transaction.SetStatus(TransactionStatuses.Scheduled);

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

            await using var tran = await DbContext.Database.BeginTransactionAsync();
            var user = await UserManager.GetUserAsync(User);

            var documentNumber = await DbContext.GetNextDocumentNumber(tran.GetDbTransaction());

            // create new transaction
            var reversal = new Transaction
            {
                Source                  = transaction.Source,
                Creator                 = user,
                TransactionDate         = DateTime.UtcNow,
                DocumentNumber          = documentNumber,
                KfsTrackingNumber       = transaction.KfsTrackingNumber,
                MerchantTrackingNumber  = transaction.MerchantTrackingNumber,
                MerchantTrackingUrl     = transaction.MerchantTrackingUrl,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
            }.SetStatus(TransactionStatuses.Scheduled);

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

            await tran.CommitAsync();

            return RedirectToAction("Details", new { id = reversal.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CallWebHook(string id)
        {
            var transaction = await DbContext.Transactions
                .Include(t => t.Source)
                .ThenInclude(s => s.Team)
                .SingleOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Source.Team.Slug != TeamSlug)
            {
                return Forbid();
            }

            var hasWebhooks = await DbContext.WebHooks.AnyAsync(w => w.Team.Slug == TeamSlug);

            if (!hasWebhooks)
            {
                return NotFound();
            }

            await WebHookService.SendWebHooksForTeam(transaction.Source.Team, new BankReconcileWebHookPayload()
            {
                KfsTrackingNumber = transaction.KfsTrackingNumber,
                MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
                TransactionDate = transaction.TransactionDate
            });

            return Ok();
        }


        private static void SanitizeTransactionsFilter(TransactionsFilterModel model)
        {
            var fromUtc = (model.From ?? DateTime.Now.AddMonths(-1)).ToUniversalTime().Date;
            var throughUtc = (model.To ?? DateTime.Now).ToUniversalTime().AddDays(1).Date;

            if (fromUtc > DateTime.UtcNow || fromUtc < DateTime.UtcNow.AddYears(-100))
            {
                // invalid, so default to filtering from one month ago
                var from = DateTime.Now.AddMonths((-1)).Date;
                model.From = from;
                fromUtc = from.ToUniversalTime();
            }
            else
            {
                model.From = fromUtc.ToLocalTime();
            }

            if (fromUtc >= throughUtc)
            {
                // invalid, so default to filtering through one month after fromUtc
                throughUtc = fromUtc.AddMonths(1).AddDays(1).Date;
                model.To = throughUtc.AddDays(-1).ToLocalTime();
            }
            else
            {
                model.To = throughUtc.ToLocalTime();
            }
        }
    }
}
