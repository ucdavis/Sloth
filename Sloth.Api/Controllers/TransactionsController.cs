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

namespace Sloth.Api.Controllers
{
    public class TransactionsController : SuperController
    {
        private readonly SlothDbContext _context;
        private readonly ILogger _logger;

        public TransactionsController(SlothDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<TransactionsController>();
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

            var transactionToCreate = new Transaction
            {
                MerchantTrackingNumber  = transaction.MerchantTrackingNumber,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
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
                }).ToList()
            };

            // create document number
            transactionToCreate.DocumentNumber = "sample";
            transactionToCreate.OriginCode = "SL";

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
