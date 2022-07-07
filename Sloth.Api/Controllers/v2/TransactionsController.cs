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
        private readonly IKfsService _kfsService;

        public TransactionsController(SlothDbContext context, IKfsService kfsService)
        {
            _context = context;
            _kfsService = kfsService;
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

            // TODO: for AE
            // make sure financial string is set
            // validate financial string

            // verify amounts, same as before

            // create new transaction, same as before

            // validate accounts
            foreach (var t in transaction.Transfers)
            {
                if (!await _kfsService.IsAccountValid(t.Chart, t.Account))
                {
                    return new BadRequestObjectResult(new
                    {
                        Message = "Invalid Chart/Account",
                        Chart = t.Chart,
                        Account = t.Account
                    });
                }
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
