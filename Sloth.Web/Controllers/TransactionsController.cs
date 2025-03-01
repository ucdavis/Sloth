using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using Sloth.Web.Models.BlobViewModels;
using Sloth.Web.Models.TransactionViewModels;
using Sloth.Web.Resources;
using Sloth.Web.Helpers;
using Microsoft.Extensions.Options;
using Sloth.Web.Models;

namespace Sloth.Web.Controllers
{

    public class TransactionsController : SuperController
    {
        private readonly IWebHookService _webHookService;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;
        private readonly DataLimitingOptions _dataLimitingOptions;

        public TransactionsController(ApplicationUserManager userManager, SlothDbContext dbContext, IWebHookService webHookService,
            IAggieEnterpriseService aggieEnterpriseService, IOptions<DataLimitingOptions> dataLimitingOptions) : base(userManager, dbContext)
        {
            _webHookService = webHookService;
            _aggieEnterpriseService = aggieEnterpriseService;
            _dataLimitingOptions = dataLimitingOptions.Value;
        }


        // GET: /<controller>/
        [Authorize(Policy = PolicyCodes.TeamAnyRole)]
        public async Task<IActionResult> Index(TransactionsFilterModel filter = null)
        {
            if (filter == null)
                filter = new TransactionsFilterModel();

            FilterHelpers.SanitizeTransactionsFilter(filter, _dataLimitingOptions.DefaultDateRange);

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
            transactions.ForEach(t => t.SetStatus(TransactionStatuses.Scheduled, details: $"Approved All by: {User.Identity.Name}"));

            // save to db
            await DbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = PolicyCodes.TeamAnyRole)]
        public async Task<IActionResult> Details(string id)
        {
            var transaction = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug)
                .Include(t => t.Scrubber)
                .Include(t => t.JournalRequest)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Include(t => t.Transfers)
                .Include(t => t.ReversalTransaction)
                .Include(t => t.ReversalOfTransaction)
                .ThenInclude(t => t.Source)
                .ThenInclude(s => s.Team)
                .Include(t => t.StatusEvents)
                .Include(t => t.Metadata)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                ErrorMessage = "Transaction not found";
                return RedirectToAction(nameof(Index));
            }

            var model = new TransactionDetailsViewModel()
            {
                Transaction = transaction,
                HasWebhooks = await DbContext.WebHooks
                    .AnyAsync(w => w.Team.Slug == TeamSlug && w.IsActive),


                RelatedTransactions = new TransactionsTableViewModel
                {
                    Transactions = await DbContext.Transactions
                        .Include(a => a.Source)
                            .ThenInclude(a => a.Team)
                        .Include(t => t.Transfers)
                        .Where(a => a.Id != transaction.Id && a.Source.Team.Slug == TeamSlug &&
                            (
                                a.KfsTrackingNumber == transaction.KfsTrackingNumber ||
                                (a.ProcessorTrackingNumber != null && a.ProcessorTrackingNumber == transaction.ProcessorTrackingNumber) ||
                                (a.MerchantTrackingNumber != null && a.MerchantTrackingNumber == transaction.MerchantTrackingNumber)
                            ))
                        .AsNoTracking()
                        .ToListAsync()
                },
                RelatedBlobs = new BlobsTableViewModel
                {
                    Blobs = await DbContext.Blobs
                        .Where(b => b.TransactionBlobs.Select(tb => tb.TransactionId).Contains(transaction.Id)
                            || b.Scrubbers.SelectMany(s => s.Transactions.Select(t => t.Id)).Contains(transaction.Id))
                        .AsNoTracking()
                        .ToListAsync(),
                    TeamSlug = TeamSlug
                },
            };

            return View(model);
        }

        [Authorize(Policy = PolicyCodes.TeamManager)]
        public async Task<IActionResult> Edit(string id)
        {
            var transaction = await DbContext.Transactions
                .Include(t => t.JournalRequest)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Include(t => t.StatusEvents)
                .Include(t => t.Transfers)
                .Include(t => t.StatusEvents)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.Source.Team.Slug == TeamSlug);

            if (transaction == null)
            {
                ErrorMessage = "Transaction not found";
                return RedirectToAction(nameof(Index));
            }

            if (transaction.Status != TransactionStatuses.PendingApproval
                && transaction.Status != TransactionStatuses.Rejected
                && !transaction.IsStale())
            {
                ErrorMessage = "Transaction is not Stale (Processing for more than 5 days), Rejected or PendingApproval";
                return RedirectToAction(nameof(Details), new { id });
            }

            foreach (var transfer in transaction.Transfers)
            {
                transfer.AccountingDate = transfer.AccountingDate?.ToPacificTime();
            }

            var model = new TransactionDetailsViewModel
            {
                Transaction = transaction,
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamManager)]
        public async Task<IActionResult> Edit(string id, TransactionEditViewModel transaction)
        {
            if (transaction == null || string.IsNullOrWhiteSpace(id))
            {
                ErrorMessage = "No transaction specified";
                return RedirectToAction(nameof(Index));
            }

            var currentTransaction = await DbContext.Transactions
                .Include(t => t.JournalRequest)
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Include(t => t.Transfers)
                .Include(t => t.StatusEvents)
                .FirstOrDefaultAsync(t => t.Id == id && t.Source.Team.Slug == TeamSlug);

            if (currentTransaction == null)
            {
                ErrorMessage = "Transaction not found";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (currentTransaction.Status != TransactionStatuses.PendingApproval
                && currentTransaction.Status != TransactionStatuses.Rejected
                && !currentTransaction.IsStale())
            {
                ErrorMessage = "Transaction is not Stale (Processing for more than 5 days), Rejected or PendingApproval";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (transaction.Transfers == null || transaction.Transfers.Count == 0)
            {
                ErrorMessage = "No transfers specified";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var ccoaValidationRequests = transaction.Transfers
                .Select(t => t.FinancialSegmentString)
                .Where(ccoa => !string.IsNullOrWhiteSpace(ccoa))
                .Distinct()
                .Select(async ccoa => new { ccoa, isValid = await _aggieEnterpriseService.IsAccountValid(ccoa, true) })
                .ToArray();

            await Task.WhenAll(ccoaValidationRequests);

            var invalidCcoaStrings = ccoaValidationRequests
                .Where(x => !x.Result.isValid)
                .Select(x => x.Result.ccoa)
                .ToHashSet();

            if (invalidCcoaStrings.Any())
            {
                ViewBag.ErrorMessage = $"Validation Failed";
            }

            var oldTransferValues = new List<TransactionEditViewModel.TransferEditModel>();

            foreach (var (transfer, edit) in
                from transfer in currentTransaction.Transfers
                from edit in transaction.Transfers.Where(x => x.Id == transfer.Id).DefaultIfEmpty()
                select (transfer, edit))
            {
                bool transferUpdated = false;
                var currentFinancialSegmentString = transfer.FinancialSegmentString;
                if (currentFinancialSegmentString != edit.FinancialSegmentString)
                {
                    transfer.FinancialSegmentString = edit.FinancialSegmentString;
                    transferUpdated = true;
                }

                if (transferUpdated)
                {
                    oldTransferValues.Add(new TransactionEditViewModel.TransferEditModel
                    {
                        Id = transfer.Id,
                        FinancialSegmentString = currentFinancialSegmentString
                    });
                }
            }

            if (string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                currentTransaction.SetStatus(TransactionStatuses.PendingApproval, $"Edited by: {User.Identity.Name} Original values changed: {JsonSerializer.Serialize(oldTransferValues)}");
                await DbContext.SaveChangesAsync();
                Log.Information("Transaction {TransactionId} edited by {User}", currentTransaction.Id, User.Identity.Name);
                Message = oldTransferValues.Count > 0 ? "Transaction updated" : "No transfers updated. Transaction set to Scheduled";
            }
            else if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                // a bit redundant, but we want to be sure that everything is updated for display
                foreach (var (transfer, edit) in
                    from transfer in currentTransaction.Transfers
                    from edit in transaction.Transfers.Where(x => x.Id == transfer.Id).DefaultIfEmpty()
                    select (transfer, edit))
                {
                    if (edit != null)
                    {
                        transfer.FinancialSegmentString = edit.FinancialSegmentString;
                        if (!string.IsNullOrWhiteSpace(edit.FinancialSegmentString) && invalidCcoaStrings.Contains(edit.FinancialSegmentString))
                        {
                            ModelState.AddModelError($"Transaction.Transfers[{currentTransaction.Transfers.IndexOf(transfer)}].FinancialSegmentString", "Invalid CCOA");
                        }
                    }
                }
                var model = new TransactionDetailsViewModel
                {
                    Transaction = currentTransaction,
                };
                return View(nameof(Edit), model);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamManager)]
        public async Task<IActionResult> Cancel(string id, string reason)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ErrorMessage = "No transaction specified";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                ErrorMessage = "No reason specified";
                return RedirectToAction(nameof(Details), new { id });
            }

            var transaction = await DbContext.Transactions
                .Include(t => t.StatusEvents)
                .FirstOrDefaultAsync(t => t.Id == id && t.Source.Team.Slug == TeamSlug);

            if (transaction == null)
            {
                ErrorMessage = "Transaction not found";
                return RedirectToAction(nameof(Index));
            }

            if (transaction.Status != TransactionStatuses.PendingApproval
                && transaction.Status != TransactionStatuses.Rejected
                && !transaction.IsStale())
            {
                ErrorMessage = "Transaction is not Stale (Processing for more than 5 days), Rejected or PendingApproval";
                return RedirectToAction(nameof(Details), new { id });
            }

            if(!string.IsNullOrWhiteSpace( transaction.ReversalOfTransactionId))
            {
                //Need to update the txn that was reversed
                var originalTransaction = await DbContext.Transactions
                    .FirstOrDefaultAsync(t => t.Id == transaction.ReversalOfTransactionId && t.Source.Team.Slug == TeamSlug);
                originalTransaction.CancelReversalTransaction(transaction);
                originalTransaction = originalTransaction.AddStatusEvent($"Reversal: {transaction.Id} Cancelled by: {User.Identity.Name} Reason: {reason}");
                DbContext.Update(originalTransaction);
            }

            transaction.SetStatus(TransactionStatuses.Cancelled, $"Cancelled by: {User.Identity.Name} Reason: {reason}");

            await DbContext.SaveChangesAsync();

            Log.Information("Transaction {TransactionId} cancelled by {User}", transaction.Id, User.Identity.Name);

            return RedirectToAction(nameof(Details), new { id });
        }


        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamApprover)]
        public async Task<IActionResult> ScheduleTransaction(string id)
        {
            var transaction = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug)
                .Include(t => t.Scrubber)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                ErrorMessage = "Transaction not found";
                return RedirectToAction(nameof(Index));
            }

            if (transaction.Status != TransactionStatuses.PendingApproval)
            {
                ErrorMessage = "Transaction is not Pending Approval";
                return RedirectToAction(nameof(Details), new { id });
            }

            transaction.SetStatus(TransactionStatuses.Scheduled, details: $"Approved by {User.Identity.Name}");

            await DbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ResubmitTransaction(string id)
        {
            var transaction = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug)
                .Include(t => t.Scrubber)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                ErrorMessage = "Transaction not found";
                return RedirectToAction(nameof(Index));
            }

            if (transaction.Status != TransactionStatuses.Rejected)
            {
                ErrorMessage = "Transaction is not Rejected";
                return RedirectToAction(nameof(Details), new { id });
            }

            transaction.SetStatus(TransactionStatuses.Scheduled, details: $"Resubmitted by {User.Identity.Name}");

            await DbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamManager)]
        public async Task<IActionResult> CreateReversal(string id, decimal reversalAmount)
        {
            reversalAmount = Math.Round(reversalAmount, 2);

            var transaction = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug)
                .Include(t => t.Scrubber)
                .Include(t => t.Source)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                ErrorMessage = "Transaction not found";
                return RedirectToAction("Index");
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

            Transaction reversal = null;

            await DbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var tran = await DbContext.Database.BeginTransactionAsync();

                var user = await UserManager.GetUserAsync(User);

                var documentNumber = await DbContext.GetNextDocumentNumber(tran.GetDbTransaction());

                // create new transaction
                reversal = new Transaction
                {
                    Source = transaction.Source,
                    Creator = user,
                    TransactionDate = DateTime.UtcNow,
                    DocumentNumber = documentNumber,
                    KfsTrackingNumber = transaction.KfsTrackingNumber,
                    MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                    MerchantTrackingUrl = transaction.MerchantTrackingUrl,
                    ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
                    Description = reversalAmount != totalAmount ? "Partial Reversal" : "Full Reversal",
                }.SetStatus(TransactionStatuses.PendingApproval).AddStatusEvent($"Transaction that was reversed: {transaction.Id}");
                

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

                // save relationship and write to the original activity line
                transaction = transaction.AddStatusEvent($"Transaction Reversed. Reversed by: {User.Identity.Name} Reversal Id: {reversal.Id}");
                transaction.AddReversalTransaction(reversal);
                await DbContext.SaveChangesAsync();
                await tran.CommitAsync();
            });

            Message = "Reversal created successfully";
            return RedirectToAction("Details", new { id = reversal.Id });
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamManager)]
        public async Task<IActionResult> CallWebHook(string id)
        {
            var transaction = await DbContext.Transactions
                .Where(t => t.Source.Team.Slug == TeamSlug)
                .Include(t => t.Source)
                .ThenInclude(s => s.Team)
                .SingleOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                ErrorMessage = "Transaction not found.";
                return RedirectToAction("Index");
            }

            var hasWebhooks = await DbContext.WebHooks.AnyAsync(w => w.Team.Slug == TeamSlug && w.IsActive);

            if (!hasWebhooks)
            {
                ErrorMessage = "Active Webhook not found for team.";
                return RedirectToAction("Details", new { id = transaction.Id });
            }

            await _webHookService.SendWebHooksForTeam(transaction.Source.Team, new BankReconcileWebHookPayload()
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
                //Try to find by id
                if (Guid.TryParse(filter.TrackingNum, out var txnId))
                {
                    var txn = await DbContext.Transactions.Where(t => t.Source.Team.Slug == TeamSlug && t.Id == filter.TrackingNum).FirstOrDefaultAsync();
                    if (txn != null)
                    {
                        Message = $"Transaction found by Id: {filter.TrackingNum}";
                        return RedirectToAction("Details", new { id = txn.Id });
                    }
                }

                //Try to find by request id
                if(Guid.TryParse(filter.TrackingNum, out var reqId))
                {
                    var txn = await DbContext.JournalRequests.Include(a => a.Transactions).Where(t => t.Source.Team.Slug == TeamSlug && t.RequestId == reqId).FirstOrDefaultAsync();
                    if (txn != null && txn.Transactions != null && txn.Transactions.Count >= 1)
                    {
                        Message = $"Transaction found by Request Id: {filter.TrackingNum}";
                        //This will work if we found an "active" journal request, but not if we replaced it.
                        return RedirectToAction("Details", new { id = txn.Transactions.First().Id });
                    }
                    if(txn != null)
                    {                        
                        Message = $"Replaced Journal Request found by Request Id: {filter.TrackingNum}";
                        return RedirectToAction("Details", new { id = txn.SavedTransactionId });
                    }
                }

            }

            if (txns.Any(a => a.ProcessorTrackingNumber == filter.TrackingNum))
            {
                if (txns.Where(a => a.ProcessorTrackingNumber == filter.TrackingNum).Count() > 1)
                {
                    Message = $"Multiple Transactions found for Processor Tracking Number: {filter.TrackingNum}. See related Txns for others.";
                }
                else
                {
                    Message = $"Processor Tracking Number found: {filter.TrackingNum}";
                }
                return RedirectToAction("Details", new { id = txns.First(a => a.ProcessorTrackingNumber == filter.TrackingNum).Id });
            }


            if (txns.Any(a => a.KfsTrackingNumber == filter.TrackingNum))
            {
                if (txns.Where(a => a.KfsTrackingNumber == filter.TrackingNum).Count() > 1)
                {
                    Message = $"Multiple Transactions found for KFS Tracking Number: {filter.TrackingNum}. See related Txns for others.";
                }
                else
                {
                    Message = $"KFS Tracking Number found: {filter.TrackingNum}";
                }
                return RedirectToAction("Details", new { txns.First(a => a.KfsTrackingNumber == filter.TrackingNum).Id });
            }


            if (txns.Any(a => a.MerchantTrackingNumber == filter.TrackingNum))
            {
                if (txns.Where(a => a.MerchantTrackingNumber == filter.TrackingNum).Count() > 1)
                {
                    Message = $"Multiple Transactions found for Merchant Tracking Number: {filter.TrackingNum}. See related Txns for others.";
                }
                else
                {
                    Message = $"Merchant Tracking Number found: {filter.TrackingNum}";
                }
                return RedirectToAction("Details", new { txns.First(a => a.MerchantTrackingNumber == filter.TrackingNum).Id });
            }

            if (User.IsInRole(Roles.SystemAdmin))
            {
                //Allow System Admins to search all transactions for specific types.
                //Try to find by id
                if (Guid.TryParse(filter.TrackingNum, out var txnId))
                {
                    var txn = await DbContext.Transactions.Include(a => a.Source.Team).Where(t => t.Id == filter.TrackingNum).FirstOrDefaultAsync();
                    if (txn != null)
                    {
                        Message = $"Different Team!!! - Transaction found by Id: {filter.TrackingNum}";
                        return RedirectToAction("Details", "Transactions", new { id = txn.Id, team = txn.Source.Team.Slug });
                    }
                }

                //Try to find by request id
                if (Guid.TryParse(filter.TrackingNum, out var reqId))
                {
                    var txn = await DbContext.JournalRequests.Include(a => a.Transactions).Include(a => a.Source).Where(t => t.RequestId == reqId).FirstOrDefaultAsync();
                    if (txn != null && txn.Transactions != null && txn.Transactions.Count >= 1)
                    {
                        var teamSlug = await DbContext.Sources.Include(a => a.Team).Where(a => a.Id == txn.Source.Id).Select(a => a.Team.Slug).FirstOrDefaultAsync();
                        Message = $"Different Team!!! - Transaction found by Request Id: {filter.TrackingNum}";
                        //This will work if we found an "active" journal request, but not if we replaced it.
                        return RedirectToAction("Details", "Transactions", new { id = txn.Transactions.First().Id, team = teamSlug });
                    }
                    if (txn != null)
                    {
                        var teamSlug = await DbContext.Sources.Include(a => a.Team).Where(a => a.Id == txn.Source.Id).Select(a => a.Team.Slug).FirstOrDefaultAsync();
                        Message = $"Different Team!!! - Replaced Journal Request found by Request Id: {filter.TrackingNum}";
                        return RedirectToAction("Details", "Transactions", new { id = txn.SavedTransactionId, team = teamSlug});
                    }
                }
            }


            ErrorMessage = $"Search Returned no results: {filter.TrackingNum}";
            return RedirectToAction("Index");

        }
    }
}
