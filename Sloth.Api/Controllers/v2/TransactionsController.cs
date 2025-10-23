using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AggieEnterpriseApi.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Sloth.Api.Attributes;
using Sloth.Api.Errors;
using Sloth.Api.Models.v2;
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
                .Include(t => t.Metadata)
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
                .Include(t => t.Metadata)
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
                .Include(t => t.Metadata)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ProcessorTrackingNumber == id);

            return transaction;
        }

        /// <summary>
        /// Fetch Transactions by Processor Tracking Number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("processortrackingnumber/{id?}")]
        [ProducesResponseType(typeof(Transaction), 200)]
        public async Task<IList<Transaction>> GetAllByProcessorId(string id)
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
                .Include(t => t.Metadata)
                .Where(t => t.ProcessorTrackingNumber == id)
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

        /// <summary>
        /// Fetch Transactions by KfsKey
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("kfskey/{id?}")]
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
                .Include(t => t.Metadata)
                .Where(t => t.KfsTrackingNumber == id)
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(IList<Transaction>), 200)]
        public async Task<IList<Transaction>> SearchByKfsKeys([FromBody] string[] ids)
        {
            var teamId = GetTeamId();

            if (ids == null || ids.Length == 0)
            {
                return new List<Transaction>();
            }

            var transactions = await _context.Transactions
                .Where(t => t.Source.Team.Id == teamId)
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Include(t => t.Metadata)
                .Where(t => ids.Contains(t.KfsTrackingNumber))
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

        /// <summary>
        /// Fetch Transactions by Metadata
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpGet("metadata/{key}/{value}")]
        [ProducesResponseType(typeof(IList<Transaction>), 200)]
        public async Task<IList<Transaction>> GetByMetadata(string key, string value)
        {
            var teamId = GetTeamId();

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                return new List<Transaction>();
            }

            var transactions = await _context.Transactions
                .Where(t => t.Source.Team.Id == teamId)
                .Include(t => t.Creator)
                .Include(t => t.Transfers)
                .Include(t => t.Metadata)
                .Where(t => t.Metadata.Any(m => m.Name == key && m.Value == value))
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

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

        /// <summary>
        /// Create a Transaction with a list of Transfers
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Transaction), 200)]
        [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
        public async Task<IActionResult> Post([FromBody] CreateTransactionViewModel transaction)
        {
            LogTransactionCreating(transaction);

            var teamId = GetTeamId();

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            // validate accounts
            foreach (var t in transaction.Transfers)
            {
                if (transaction.ValidateFinancialSegmentStrings)
                {
                    try
                    {
                        // do full segment validation via the API
                        if (!await _aggieEnterpriseService.IsAccountValid(t.FinancialSegmentString, true))
                        {
                            return new BadRequestObjectResult(new
                            {
                                Message = "Invalid Chart String",
                                t.FinancialSegmentString
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error validating financial segment string: {FinancialSegmentString}", t.FinancialSegmentString);
                        return new BadRequestObjectResult(new
                        {
                            Message = "Error validating Chart String",
                            t.FinancialSegmentString
                        });
                    }
                }
                else
                {
                    // skip full validation, but still ensure the account format is correct
                    if (FinancialChartValidation.GetFinancialChartStringType(t.FinancialSegmentString) ==
                        FinancialChartStringType.Invalid)
                    {
                        return new BadRequestObjectResult(new
                        {
                            Message = "Invalid Chart String Format",
                            t.FinancialSegmentString
                        });
                    }
                }
            }


            // valid fiscal period consistency
            var accountingDateCount = transaction.Transfers.Select(t => t.AccountingDate).Distinct().Count();

            // we only allow a single fiscal period per transaction
            if (accountingDateCount != 1)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Invalid Accounting Date - must be the same for all transfers"
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
                return new BadRequestObjectResult(new
                {
                    Message = "Credit/Debit Amounts don't match",
                    Credits = creditTotal,
                    Debits = debitTotal,
                });
            }

            // find source
            var source = await _context.Sources.FirstOrDefaultAsync(s =>
                s.Name == transaction.Source && s.Type == transaction.SourceType && s.Team.Id == teamId);

            if (source == null)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Source not found",
                    Source = transaction.Source,
                    Type = transaction.SourceType
                });
            }

            // create final transaction
            var transactionToCreate = new Transaction
            {
                MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                ProcessorTrackingNumber = transaction.ProcessorTrackingNumber,
                MerchantTrackingUrl = transaction.MerchantTrackingUrl,
                KfsTrackingNumber = transaction.KfsTrackingNumber,
                Source = source,
                TransactionDate = DateTime.UtcNow,
                Description = transaction.Description,
                Transfers = transaction.Transfers.Select(t => new Transfer()
                {
                    Amount = t.Amount,
                    FinancialSegmentString = t.FinancialSegmentString,
                    Description = t.Description,
                    Direction = t.Direction,
                    AccountingDate = t.AccountingDate,
                    ReferenceId = t.ReferenceId,
                }).ToList(),
            };

            transactionToCreate.SetStatus(transaction.AutoApprove
                ? TransactionStatuses.Scheduled
                : TransactionStatuses.PendingApproval);

            // assign metadata if it exists
            if (transaction.Metadata is { Count: > 0 })
            {
                foreach (var entry in transaction.Metadata)
                {
                    transactionToCreate.Metadata.Add(new TransactionMetadata { Transaction = transactionToCreate, Name = entry.Name, Value = entry.Value });
                }
            }


            await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var txn = await _context.Database.BeginTransactionAsync();

                // create document number
                transactionToCreate.DocumentNumber = await _context.GetNextDocumentNumber(txn.GetDbTransaction());

                // create kfs number if necessary
                if (string.IsNullOrWhiteSpace(transactionToCreate.KfsTrackingNumber))
                {
                    transactionToCreate.KfsTrackingNumber = await _context.GetNextKfsTrackingNumber(txn.GetDbTransaction());
                }

                _context.Transactions.Add(transactionToCreate);
                await _context.SaveChangesAsync();
                await txn.CommitAsync();
            });

            return new JsonResult(transactionToCreate);
        }

        private void LogTransactionCreating(CreateTransactionViewModel transaction)
        {
            var txn = new
            {
                transaction.KfsTrackingNumber,
                transaction.ProcessorTrackingNumber,
                transaction.MerchantTrackingNumber,
                transaction.MerchantTrackingUrl,
                transaction.Source,
                transaction.SourceType,
                Transfers = transaction.Transfers.Select(t => new { t.Amount, t.FinancialSegmentString, t.Description, t.Direction }).ToArray(),
                Metadata = transaction.Metadata.Select(m => new { m.Name, m.Value }).ToArray(),
            };

            Log.Information("Transaction Creation Starting: {@Transaction}", txn);
        }

        private string GetTeamId()
        {
            return User.FindFirst(ClaimTypes.PrimaryGroupSid)?.Value;
        }
    }
}
