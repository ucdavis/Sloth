using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sloth.Api.Attributes;
using Sloth.Api.Errors;
using Sloth.Api.Models;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Api.Controllers.v2
{
    [Authorize(Policy = "ApiKey")]
    [VersionedRoute("2", "[controller]")]
    [ApiExplorerSettings(GroupName = "v2")]
    [ProducesResponseType(typeof(InternalExceptionResponse), 500)]
    public class TransactionsController : Controller
    {
        private readonly SlothDbContext _context;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public TransactionsController(SlothDbContext context, IAggieEnterpriseService aggieEnterpriseService)
        {
            _context = context;
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        /// <summary>
        /// Fetch Top 1 Transaction
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<Transaction>), 200)]
        public async Task<IList<Transaction>> Get()
        {
            var teamId = GetTeamId();

            var transactions = await _context.Transactions
                .Where(t => t.Source.Team.Id == teamId)
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Take(1)
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

        /// <summary>
        /// Fetch Transaction by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Transaction), 200)]
        public async Task<Transaction> Get(string id)
        {
            var teamId = GetTeamId();

            var transaction = await _context.Transactions
                .Where(t => t.Source.Team.Id == teamId)
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            return transaction;
        }

        /// <summary>
        /// Fetch Transaction by Processor Tracking Number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("processor/{id}")]
        [ProducesResponseType(typeof(Transaction), 200)]
        public async Task<Transaction> GetByProcessorId(string id)
        {
            var teamId = GetTeamId();

            var transaction = await _context.Transactions
                .Where(t => t.Source.Team.Id == teamId)
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ProcessorTrackingNumber == id);

            return transaction;
        }

        /// <summary>
        /// Fetch Transactions by KfsKey
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("kfskey/{id}")]
        [ProducesResponseType(typeof(IList<Transaction>), 200)]
        public async Task<IList<Transaction>> GetByKfsKey(string id)
        {
            var teamId = GetTeamId();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new List<Transaction>();
            }

            var transactions = await _context.Transactions
                .Where(t => t.Source.Team.Id == teamId)
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Where(t => t.KfsTrackingNumber == id)
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

        // TODO: just for testing, remove later

        /// <summary>
        /// Validate Financial Segment String
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("validate/{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<bool> ValidateFinancialSegmentString(string id)
        {
            return await _aggieEnterpriseService.IsAccountValid(id);
        }


        // TODO: update to AE

        /// <summary>
        /// Create a Transaction with a list of Transfers
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Transaction), 200)]
        [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
        public async Task<IActionResult> Post([FromBody]CreateTransactionViewModel transaction)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            // TODO: currently just validating fake string, need to update to AE
            // validate accounts
            foreach (var t in transaction.Transfers)
            {
                if (!await _aggieEnterpriseService.IsAccountValid("coa-here", true))
                {
                    // TODO: do we want to return the error message if invalid?
                    return new BadRequestObjectResult(new
                    {
                        Message = "Invalid Chart String",
                        FinancialSegmentString = "COA here"
                    });
                }
            }

            // valid fiscal period consistency
            var fiscalPeriodCount = transaction.Transfers.Select(t=> new { t.FiscalPeriod, t.FiscalYear }).Distinct().Count();
            
            // we only allow a single fiscal period per transaction
            if (fiscalPeriodCount != 1) {
                return new BadRequestObjectResult(new
                {
                    Message = "Invalid Fiscal Periods - must be the same for all transfers"
                });
            }

            // validate amounts
            var badDecimalAmounts = transaction.Transfers
                .Where(t => t.Amount != Math.Round(t.Amount, 2))
                .Select(t => t.Amount.ToString(CultureInfo.InvariantCulture)).ToArray();

            if (badDecimalAmounts.Any())
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Credit/Debit amount(s) with more than two decimal places",
                    Amounts = string.Join(", ", badDecimalAmounts)
                });
            }

            var creditTotal = transaction.Transfers
                .Where(t => t.Direction == Transfer.CreditDebit.Credit)
                .Sum(t => t.Amount);

            var debitTotal = transaction.Transfers
                .Where(t => t.Direction == Transfer.CreditDebit.Debit)
                .Sum(t => t.Amount);

            if (creditTotal != debitTotal)
            {
                return new BadRequestObjectResult(new {
                    Message = "Credit/Debit Amounts don't match",
                    Credits = creditTotal,
                    Debits = debitTotal,
                });
            }

            // find source
            var source = await _context.Sources.FirstOrDefaultAsync(s =>
                s.Name == transaction.Source && s.Type == transaction.SourceType);

            if (source == null)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Source not found",
                    Source = transaction.Source,
                    Type = transaction.SourceType
                });
            }

            // TODO: financial info needs to be update to AE
            // create final transaction
            var transactionToCreate = new Transaction
            {
                MerchantTrackingNumber  = transaction.MerchantTrackingNumber,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
                MerchantTrackingUrl     = transaction.MerchantTrackingUrl,
                KfsTrackingNumber       = transaction.KfsTrackingNumber,
                Source                  = source,
                TransactionDate         = transaction.TransactionDate,
                Transfers               = transaction.Transfers.Select(t => new Transfer()
                {
                    Account       = t.Account,
                    Amount        = t.Amount,
                    Chart         = t.Chart,
                    Description   = t.Description,
                    Direction     = t.Direction,
                    FiscalPeriod  = t.FiscalPeriod ?? DateTime.UtcNow.GetFiscalPeriod(),
                    FiscalYear    = t.FiscalYear ?? DateTime.UtcNow.GetFinancialYear(),
                    ObjectCode    = t.ObjectCode,
                    ObjectType    = t.ObjectType,
                    Project       = t.Project,
                    ReferenceId   = t.ReferenceId,
                    SubAccount    = t.SubAccount,
                    SubObjectCode = t.SubObjectCode,
                }).ToList(),
            };

            transactionToCreate.SetStatus(transaction.AutoApprove
                ? TransactionStatuses.Scheduled
                : TransactionStatuses.PendingApproval);

            using (var tran = _context.Database.BeginTransaction())
            {
                // create document number
                transactionToCreate.DocumentNumber = await _context.GetNextDocumentNumber(tran.GetDbTransaction());

                // create kfs number if necessary
                if (string.IsNullOrWhiteSpace(transactionToCreate.KfsTrackingNumber))
                {
                    transactionToCreate.KfsTrackingNumber = await _context.GetNextKfsTrackingNumber(tran.GetDbTransaction());
                } 

                _context.Transactions.Add(transactionToCreate);
                await _context.SaveChangesAsync();
                tran.Commit();

                return new JsonResult(transactionToCreate); 
            }
        }
        
        private string GetTeamId() {
            return User.FindFirst(ClaimTypes.PrimaryGroupSid).Value;
        }
    }
}
