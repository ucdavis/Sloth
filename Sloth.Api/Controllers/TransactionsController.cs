using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            _logger = loggerFactory.CreateLogger<ScrubbersController>();
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
        /// Create a Transaction with a list of Transfers
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Transaction), 200)]
        public async Task<IActionResult> Post([FromBody]CreateTransactionViewModel transaction)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            var transactionToCreate = new Transaction
            {
                MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
                TransactionDate = transaction.TransactionDate,
                Transfers = transaction.Transfers
            };

            _context.Transactions.Add(transactionToCreate);
            await _context.SaveChangesAsync();

            return new JsonResult(transactionToCreate);
        }
    }
}
