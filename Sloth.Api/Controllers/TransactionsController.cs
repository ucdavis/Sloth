using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Sloth.Api.Helpers;
using Sloth.Api.Models;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Api.Controllers
{
    public class TransactionsController : SuperController
    {
        private readonly SlothDbContext _context;
        private readonly IKfsService _kfsService;

        public TransactionsController(SlothDbContext context, IKfsService kfsService)
        {
            _context = context;
            _kfsService = kfsService;
        }

        /// <summary>
        /// Fetch Top 1 Transactions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<Transaction>), 200)]
        public async Task<IList<Transaction>> Get()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Include(t => t.Scrubber)
                .Take(1)
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

        /// <summary>
        /// Fetch Transactions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Transaction), 200)]
        public async Task<Transaction> Get(string id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Include(t => t.Scrubber)
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
            var transaction = await _context.Transactions
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Include(t => t.Scrubber)
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
            if (string.IsNullOrWhiteSpace(id))
            {
                return new List<Transaction>();
            }

            var transactions = await _context.Transactions
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Include(t => t.Scrubber)
                .Where(t => t.KfsTrackingNumber == id)
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

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

            // find source
            var source = await _context.Sources.FirstOrDefaultAsync(s =>
                string.Equals(s.Name, transaction.Source, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(s.Type, transaction.SourceType, StringComparison.InvariantCultureIgnoreCase));

            if (source == null)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Source not found",
                    Source = transaction.Source,
                    Type = transaction.SourceType
                });
            }

            // create document number
            var documentNumber = "sample";

            var transactionToCreate = new Transaction
            {
                MerchantTrackingNumber  = transaction.MerchantTrackingNumber,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
                Source                  = source,
                DocumentNumber          = documentNumber,
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

            if (transaction.AutoApprove)
            {
                transactionToCreate.Status = TransactionStatuses.Scheduled;
            }
            else
            {
                transactionToCreate.Status = TransactionStatuses.PendingApproval;
            }

            using (var tran = _context.Database.BeginTransaction())
            {
                // create kfs number
                transactionToCreate.KfsTrackingNumber = await _context.GetNextKfsTrackingNumber(tran.GetDbTransaction());

                _context.Transactions.Add(transactionToCreate);
                await _context.SaveChangesAsync();
                tran.Commit();

                return new JsonResult(transactionToCreate); 
            }
        }
    }
}
