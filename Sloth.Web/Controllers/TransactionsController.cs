using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Models.TransactionViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    
    public class TransactionsController : SuperController
    {
        private readonly IWebHookService WebHookService;

        public TransactionsController(ApplicationUserManager userManager, SlothDbContext dbContext, IWebHookService webHookService) : base(userManager, dbContext)
        {
            WebHookService = webHookService;
        }


        // GET: /<controller>/
        [Authorize(Policy = PolicyCodes.TeamAnyRole)]
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
                    teamMerchantIds.Select(i => new SelectListItem() { Value = i, Text = i }).ToList();
            }

            result.TransactionsTable = new TransactionsTableViewModel()
            {
                Transactions = await query
                    .Include(t => t.Transfers)
                    .AsNoTracking()
                    .ToListAsync()
            };

            result.PendingApprovalCount = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug && t.Status == TransactionStatuses.PendingApproval)
                .CountAsync();

            var team = await DbContext.Teams.FirstAsync(t => t.Slug == TeamSlug);
            ViewBag.Title = $"Transactions - {team.Name}";

            return View("Index", result);
        }

        [Authorize(Policy = PolicyCodes.TeamApprover)]
        public async Task<IActionResult> NeedApproval()
        {
            var transactionsTable = new TransactionsTableViewModel()
            {
                Transactions = await DbContext.Transactions
                    .Include(t => t.Transfers)
                    .Where(t => t.Source.Team.Slug == TeamSlug)
                    .Where(t => t.Status == TransactionStatuses.PendingApproval)
                    .AsNoTracking()
                    .ToListAsync()
            };

            return View(transactionsTable);
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamApprover)]
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

        [Authorize(Policy = PolicyCodes.TeamAnyRole)]
        public async Task<IActionResult> Details(string id)
        {
            var transaction = await DbContext.Transactions
                .Include(t => t.Scrubber)
                .Include(t => t.JournalRequest)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Include(t => t.Transfers)
                .Include(t => t.ReversalTransaction)
                .Include(t => t.ReversalOfTransaction)
                .Include(t => t.StatusEvents)
                .FirstOrDefaultAsync(t => t.Id == id);

            var relatedTransactions = await DbContext.Transactions
                .Include(a => a.Source)
                    .ThenInclude(a => a.Team)
                .Include(t => t.Transfers)
                .Where(a => a.Id != transaction.Id && a.Source.Team.Slug == TeamSlug &&
                    (
                        a.KfsTrackingNumber == transaction.KfsTrackingNumber ||
                        (a.ProcessorTrackingNumber != null && a.ProcessorTrackingNumber == transaction.ProcessorTrackingNumber) ||
                        a.MerchantTrackingNumber == transaction.MerchantTrackingNumber
                    )
                ).ToListAsync();

            var model = new TransactionDetailsViewModel()
            {
                Transaction = transaction,
                HasWebhooks = await DbContext.WebHooks
                    .AnyAsync(w => w.Team.Slug == TeamSlug && w.IsActive),


                RelatedTransactions = new TransactionsTableViewModel
                {
                    Transactions = relatedTransactions
                }
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamApprover)]
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
        [Authorize(Policy = PolicyCodes.TeamManager)]
        public async Task<IActionResult> CreateReversal(string id, decimal reversalAmount)
        {
            reversalAmount = Math.Round(reversalAmount, 2);

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
                ErrorMessage = "Cannot reverse incomplete transaction";
                return RedirectToAction("Details", new { id });
            }

            // you can only reverse a transaction once
            if (transaction.HasReversal)
            {
                ErrorMessage = "Cannot reverse transaction again";
                return RedirectToAction("Details", new { id });
            }

            if (reversalAmount < 0.01m)
            {
                ErrorMessage = "Reversal amount must be greater than 0";
                return RedirectToAction("Details", new { id });
            }

            var totalAmount = transaction.Transfers
                .Where(t => t.Direction == Transfer.CreditDebit.Credit)
                .Sum(t => t.Amount);
            if (reversalAmount > totalAmount)
            {
                ErrorMessage = "Cannot reverse more than the total amount of the transaction";
                return RedirectToAction("Details", new { id });
            }

            var percentage = reversalAmount / totalAmount;

            await using var tran = await DbContext.Database.BeginTransactionAsync();
            var user = await UserManager.GetUserAsync(User);

            var documentNumber = await DbContext.GetNextDocumentNumber(tran.GetDbTransaction());

            // create new transaction
            var reversal = new Transaction
            {
                Source = transaction.Source,
                Creator = user,
                TransactionDate = DateTime.UtcNow,
                DocumentNumber = documentNumber,
                KfsTrackingNumber = transaction.KfsTrackingNumber,
                MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                MerchantTrackingUrl = transaction.MerchantTrackingUrl,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
            }.SetStatus(TransactionStatuses.Scheduled);

            // add reversal transfers
            foreach (var transfer in transaction.Transfers)
            {
                var amount = Math.Round(transfer.Amount * percentage, 2);
                // don't create a transfer if it rounds to less than one cent
                if (amount < 0.01m)
                {
                    continue;
                }

                // same info, except reverse direction
                var direction = transfer.Direction == Transfer.CreditDebit.Credit
                    ? Transfer.CreditDebit.Debit
                    : Transfer.CreditDebit.Credit;

                reversal.Transfers.Add(new Transfer
                {
                    Amount = amount,
                    Account = transfer.Account,
                    Chart = transfer.Chart,
                    Description = transfer.Description,
                    Direction = direction,
                    FiscalPeriod = DateTime.UtcNow.GetFiscalPeriod(),
                    FiscalYear = DateTime.UtcNow.GetFinancialYear(),
                    ObjectCode = transfer.ObjectCode,
                    ObjectType = transfer.ObjectType,
                    Project = transfer.Project,
                    ReferenceId = transfer.ReferenceId,
                    SequenceNumber = transfer.SequenceNumber,
                    FinancialSegmentString = transfer.FinancialSegmentString,
                    SubAccount = transfer.SubAccount,
                    SubObjectCode = transfer.SubObjectCode,
                });
            }

            var reversalCredits = reversal.Transfers.Where(t => t.Direction == Transfer.CreditDebit.Credit).ToArray();
            var reversalDebits = reversal.Transfers.Where(t => t.Direction == Transfer.CreditDebit.Debit).ToArray();
            var totalReversalCredit = reversalCredits.Sum(t => t.Amount);
            var totalReversalDebit = reversalDebits.Sum(t => t.Amount);

            /// adjust for rounding error
            if (totalReversalDebit > totalReversalCredit)
            {
                var centIncrements = (int)Math.Round((totalReversalDebit - totalReversalCredit) * 100);
                for (var i = 0; i < centIncrements; i++)
                {
                    reversalCredits[i % reversalCredits.Length].Amount += 0.01m;
                }
            }
            else if (totalReversalCredit > totalReversalDebit)
            {
                var centIncrements = (int)Math.Round((totalReversalCredit - totalReversalDebit) * 100);
                for (var i = 0; i < centIncrements; i++)
                {
                    reversalDebits[i % reversalDebits.Length].Amount += 0.01m;
                }
            }

            // save transaction to establish id
            await DbContext.Transactions.AddAsync(reversal);
            await DbContext.SaveChangesAsync();

            // save relationship
            transaction.AddReversalTransaction(reversal);
            await DbContext.SaveChangesAsync();

            await tran.CommitAsync();

            Message = "Reversal created successfully";
            return RedirectToAction("Details", new { id = reversal.Id });
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamManager)]
        public async Task<IActionResult> CallWebHook(string id)
        {
            var transaction = await DbContext.Transactions
                .Include(t => t.Source)
                .ThenInclude(s => s.Team)
                .SingleOrDefaultAsync(t => t.Id == id);
           
            if (transaction == null)
            {
                ErrorMessage = "Transaction not found.";
                return RedirectToAction("Index");
            }

            if (transaction.Source.Team.Slug != TeamSlug)
            {
                ErrorMessage = $"Team Mismatch {transaction.Source.Team.Slug} not {TeamSlug}";
                return RedirectToAction("Index", "Home");
                
            }

            var hasWebhooks = await DbContext.WebHooks.AnyAsync(w => w.Team.Slug == TeamSlug && w.IsActive);

            if (!hasWebhooks)
            {
                ErrorMessage = "Active Webhook not found for team.";
                return RedirectToAction("Details", new {id=transaction.Id});
            }

            await WebHookService.SendWebHooksForTeam(transaction.Source.Team, new BankReconcileWebHookPayload()
            {
                KfsTrackingNumber = transaction.KfsTrackingNumber,
                MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
                TransactionDate = transaction.TransactionDate
            });

            Message = "Webhook called.";

            return RedirectToAction("Details", new { id = transaction.Id });
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamAnyRole)]
        public async Task<IActionResult> Search(TransactionsFilterModel filter = null)
        {
            if (string.IsNullOrWhiteSpace(filter?.TrackingNum))
            {
                return RedirectToAction("Index");
            }

            var txns = await DbContext.Transactions.Where(t => t.Source.Team.Slug == TeamSlug
                                && (t.ProcessorTrackingNumber == filter.TrackingNum
                                    || t.KfsTrackingNumber == filter.TrackingNum
                                    || t.MerchantTrackingNumber == filter.TrackingNum)).OrderBy(a => a.TransactionDate).ToListAsync();
            if (txns == null || txns.Count <= 0)
            {
                ErrorMessage = $"Search Returned no results: {filter.TrackingNum}";
                return RedirectToAction("Index");

            }

            if (txns.Any(a => a.ProcessorTrackingNumber == filter.TrackingNum))
            {
                Message = $"Processor Tracking Number found: {filter.TrackingNum}";
                return RedirectToAction("Details", new {id = txns.First(a => a.ProcessorTrackingNumber == filter.TrackingNum).Id});
            }


            if (txns.Any(a => a.KfsTrackingNumber == filter.TrackingNum))
            {
                Message = $"KFS Tracking Number found: {filter.TrackingNum}";
                return RedirectToAction("Details", new { txns.First(a => a.KfsTrackingNumber == filter.TrackingNum).Id });
            }


            if (txns.Any(a => a.MerchantTrackingNumber == filter.TrackingNum))
            {
                Message = $"Merchant Tracking Number found: {filter.TrackingNum}";
                return RedirectToAction("Details", new { txns.First(a => a.MerchantTrackingNumber == filter.TrackingNum).Id });
            }

            ErrorMessage = $"Search Returned no results: {filter.TrackingNum}";
            return RedirectToAction("Index");

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
