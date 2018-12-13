using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Serilog;
using Sloth.Core.Configuration;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Integrations.Cybersource;
using Sloth.Integrations.Cybersource.Clients;

namespace Sloth.Core.Jobs
{
    public class CybersourceBankReconcileJob
    {
        private readonly SlothDbContext _context;
        private readonly ISecretsService _secretsService;
        private readonly CybersourceOptions _options;

        public static string JobName = "Cybersource.BankReconcile";

        public CybersourceBankReconcileJob(SlothDbContext context, ISecretsService secretsService, IOptions<CybersourceOptions> options)
        {
            _context = context;
            _secretsService = secretsService;
            _options = options.Value;

            // TODO validate options
        }

        public async Task ProcessReconcile(ILogger log, DateTime date)
        {
            log = log.ForContext("date", date);

            try
            {
                var integrations = _context.Integrations
                    .Where(i => i.Type == IntegrationTypes.CyberSource)
                    .Include(i => i.Source)
                    .ToList();

                if (!integrations.Any())
                {
                    log.Information("Early exit, no active integrations found.");
                }

                foreach (var integration in integrations)
                {
                    await ProcessIntegration(log, integration, date);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
            }
        }

        private async Task ProcessIntegration(ILogger log, Integration integration, DateTime yesterday)
        {
            log = log.ForContext("integration", integration.Id);

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
                        // moving monies from clearing to holding

                        // create document number
                        var documentNumber = await _context.GetNextDocumentNumber(tran.GetDbTransaction());
                        var kfsTrackingNumber = await _context.GetNextKfsTrackingNumber(tran.GetDbTransaction());

                        transaction = new Transaction()
                        {
                            Source                  = integration.Source,
                            Status                  = TransactionStatuses.Scheduled,
                            DocumentNumber          = documentNumber,
                            KfsTrackingNumber       = kfsTrackingNumber,
                            MerchantTrackingNumber  = deposit.MerchantReferenceNumber,
                            ProcessorTrackingNumber = deposit.RequestID,
                            TransactionDate         = yesterday,
                        };

                        // move money out of clearing
                        var clearing = new Transfer()
                        {
                            Chart        = "3",
                            Account      = integration.ClearingAccount,
                            Direction    = Transfer.CreditDebit.Debit,
                            Amount       = deposit.Amount,
                            Description  = "Cybersource Deposit",
                            ObjectCode   = ObjectCodes.Income,
                        };
                        transaction.Transfers.Add(clearing);

                        // move money into holding
                        var holding = new Transfer()
                        {
                            Chart        = "3",
                            Account      = integration.HoldingAccount,
                            Direction    = Transfer.CreditDebit.Credit,
                            Amount       = deposit.Amount,
                            Description  = "Cybersource Deposit",
                            ObjectCode   = ObjectCodes.Income,
                        };
                        transaction.Transfers.Add(holding);

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
