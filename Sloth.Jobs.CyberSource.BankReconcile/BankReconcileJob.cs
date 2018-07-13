using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Serilog;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Integrations.Cybersource;
using Sloth.Integrations.Cybersource.Clients;

namespace Sloth.Jobs.CyberSource.BankReconcile
{
    public class BankReconcileJob
    {
        private readonly ILogger _log;
        private readonly SlothDbContext _context;
        private readonly ISecretsService _secretsService;
        private readonly CybersourceOptions _options;

        public BankReconcileJob(ILogger log, SlothDbContext context, ISecretsService secretsService, IOptions<CybersourceOptions> options)
        {
            _log = log;
            _context = context;
            _secretsService = secretsService;
            _options = options.Value;

            // validate options
            if (string.IsNullOrWhiteSpace(_options.ClearingAccount) ||
                _options.ClearingAccount.Length > 7)
            {
                throw new ArgumentException("ClearingAccount must be non-null and less than 7 characters.");
            }

            if (string.IsNullOrWhiteSpace(_options.HoldingAccount) ||
                _options.ClearingAccount.Length > 7)
            {
                throw new ArgumentException("HoldingAccount must be non-null and less than 7 characters.");
            }
        }

        public async Task UploadScrubber()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var log = _log.ForContext("date", yesterday);

            try
            {
                var integrations = _context.Integrations
                    .Where(i => i.Type == IntegrationTypes.CyberSource)
                    .Include(i => i.Source)
                    .ToList();

                foreach (var integration in integrations)
                {
                    await ProcessIntegration(integration, yesterday);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                throw;
            }
        }

        private async Task ProcessIntegration(Integration integration, DateTime yesterday)
        {
            var log = _log.ForContext("date", yesterday)
                          .ForContext("integration", integration.Id);

            using (var tran = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // fetch password secret
                    var password = await _secretsService.GetSecret(integration.ReportPasswordKey);

                    // create client
                    var client = new ReportClient(_options.ReportUrl, integration.MerchantId,
                        integration.ReportUsername,
                        password);

                    // fetch report
                    var report = await client.GetPaymentBatchDetailReport(yesterday);
                    var count = report.Batches?.Batch?.Length ?? 0;

                    log.Information("Report found with {count} records.", new { count });

                    if (count < 1)
                    {
                        return;
                    }

                    // setup values
                    var fiscalYear = yesterday.FiscalYear();
                    var fiscalPeriod = yesterday.FiscalPeriod();

                    // iterate over deposits
                    foreach (var deposit in report.Batches?.Batch?.SelectMany(b => b.Requests?.Request) ??
                                            new List<Request>())
                    {
                        // look for existing transaction
                        var transaction =
                            _context.Transactions.FirstOrDefault(t => t.ProcessorTrackingNumber == deposit.RequestID);
                        if (transaction != null)
                        {
                            log.ForContext("tracking_number", deposit.MerchantReferenceNumber)
                                .Information("Transaction already exists.");
                            continue;
                        }

                        log.ForContext("tracking_number", deposit.MerchantReferenceNumber)
                            .Information("Creating transaction");

                        // create transaction per deposit item,
                        // moving monies from clearing to holding then final acocunt
                        var kfsTrackingNumber = await _context.GetNextKfsTrackingNumber(tran.GetDbTransaction());

                        transaction = new Transaction()
                        {
                            Source = integration.Source,
                            Status = TransactionStatuses.Scheduled,
                            KfsTrackingNumber = kfsTrackingNumber,
                            MerchantTrackingNumber = deposit.MerchantReferenceNumber,
                            ProcessorTrackingNumber = deposit.RequestID,
                            DocumentNumber = "ADOCUMENT1",
                            TransactionDate = yesterday,
                        };

                        // move money out of clearing
                        var clearing = new Transfer()
                        {
                            Chart = "3",
                            Account = _options.ClearingAccount,
                            Direction = Transfer.CreditDebit.Debit,
                            Amount = deposit.Amount,
                            Description = "Deposit",
                            ObjectCode = "ABCD",
                            FiscalYear = fiscalYear,
                            FiscalPeriod = fiscalPeriod,
                        };
                        transaction.Transfers.Add(clearing);

                        // move money into holding
                        var holding = new Transfer()
                        {
                            Chart = "3",
                            Account = _options.HoldingAccount,
                            Direction = Transfer.CreditDebit.Credit,
                            Amount = deposit.Amount,
                            Description = "Deposit",
                            ObjectCode = "ABCD",
                            FiscalYear = fiscalYear,
                            FiscalPeriod = fiscalPeriod,
                        };
                        transaction.Transfers.Add(holding);

                        // then back out of holding
                        var holding2 = new Transfer()
                        {
                            Chart = "3",
                            Account = _options.HoldingAccount,
                            Direction = Transfer.CreditDebit.Debit,
                            Amount = deposit.Amount,
                            Description = "Deposit",
                            ObjectCode = "ABCD",
                            FiscalYear = fiscalYear,
                            FiscalPeriod = fiscalPeriod,
                        };
                        transaction.Transfers.Add(holding2);

                        // into the default account
                        var final = new Transfer()
                        {
                            Chart = "3",
                            Account = integration.DefaultAccount,
                            Direction = Transfer.CreditDebit.Credit,
                            Amount = deposit.Amount,
                            Description = "Deposit",
                            ObjectCode = "ABCD",
                            FiscalYear = fiscalYear,
                            FiscalPeriod = fiscalPeriod,
                        };
                        transaction.Transfers.Add(final);

                        var errors = _context.ValidateModel(transaction);
                        if (errors.Any())
                        {
                            log.ForContext("errors", errors).Warning("Validation Errors Detected");
                            throw new Exception("Validation Errors Detected");
                        }

                        _context.Transactions.Add(transaction);
                    }

                    // push changes for this integration
                    var inserted = await _context.SaveChangesAsync();
                    tran.Commit();
                    log.Information("{count} records created.", new { count = inserted });
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    tran.Rollback();
                }
            }
        }
    }
}
