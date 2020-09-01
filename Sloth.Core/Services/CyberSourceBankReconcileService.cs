using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Serilog;
using Sloth.Core.Configuration;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;
using Sloth.Core.Resources;
using Sloth.Integrations.CyberSource;
using Sloth.Integrations.Cybersource.Clients;
using Sloth.Integrations.Cybersource.Helpers;
using Sloth.Integrations.Cybersource.Resources;

namespace Sloth.Core.Services
{
    public interface ICyberSourceBankReconcileService
    {
        Task ProcessIntegration(Integration integration, DateTime date, ILogger log = null);

        Task ProcessOneTimeIntegration(Integration integration, string reportName, DateTime date, ILogger log = null);
    }

    public class CyberSourceBankReconcileService : ICyberSourceBankReconcileService
    {
        private readonly SlothDbContext _context;
        private readonly ISecretsService _secretsService;
        private readonly IWebHookService _webHookService;
        private readonly CybersourceOptions _options;
        private readonly IStorageService _storageService;

        public CyberSourceBankReconcileService(SlothDbContext context, ISecretsService secretsService,
            IWebHookService webHookService, IOptions<CybersourceOptions> options, IStorageService storageService)
        {
            _context = context;
            _secretsService = secretsService;
            _webHookService = webHookService;
            _options = options.Value;
            _storageService = storageService;

            // TODO validate options
        }

        public async Task ProcessIntegration(Integration integration, DateTime date, ILogger log = null)
        {
            if (log == null)
            {
                log = Log.Logger;
            }

            log = log.ForContext("integration", integration.Id);
            
            // fetch password secret
            var password = await _secretsService.GetSecret(integration.ReportPasswordKey);

            // create client
            var client = new ReportClient(_options.IsProduction, integration.MerchantId,
                integration.ReportUsername,
                password);

            // fetch report
            var (report, reportXml) = await client.GetPaymentBatchDetailReport(date);

            var count = report.Requests?.Length ?? 0;

            log.Information("Report found with {count} records.", new { count });

            if (count < 1)
            {
                return;
            }

            await ProcessReport(report, reportXml, integration, log);
        }

        public async Task ProcessOneTimeIntegration(Integration integration, string reportName, DateTime date, ILogger log = null)
        {
            if (log == null)
            {
                log = Log.Logger;
            }

            log = log.ForContext("integration", integration.Id);

            // fetch password secret
            var password = await _secretsService.GetSecret(integration.ReportPasswordKey);

            // create client
            var client = new ReportClient(_options.IsProduction, integration.MerchantId,
                integration.ReportUsername,
                password);

            // fetch report
            var (report, reportXml) = await client.GetOneTimePaymentBatchDetailReport(reportName, date);

            var count = report.Requests?.Length ?? 0;

            log.Information("Report found with {count} records.", new { count });

            if (count < 1)
            {
                return;
            }

            await ProcessReport(report, reportXml, integration, log);
        }

        private async Task ProcessReport(Report report, string reportXml, Integration integration, ILogger log)
        {
            using (var tran = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // iterate over deposits
                    foreach (var deposit in report.Requests?.ToList() ?? new List<ReportRequest>())
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

                        // transfers can often have multiple application replies, locate the correct one
                        var replyIndex = deposit.ApplicationReplies.IndexOf(r => r.Name == ApplicationReplyTypes.Bill);
                        if (replyIndex == -1)
                        {
                            log.ForContext("tracking_number", deposit.MerchantReferenceNumber)
                                .Information("Reply Type Bill not found. (Maybe a refund). Skipping.");
                            continue;
                        }
                        var paymentInfo = deposit.PaymentData[replyIndex];

                        var amount = decimal.Parse(paymentInfo.Amount);

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
                            TransactionDate         = deposit.LocalizedRequestDate,
                        };

                        // move money out of clearing
                        var clearing = new Transfer()
                        {
                            Chart       = "3",
                            Account     = integration.ClearingAccount,
                            Direction   = Transfer.CreditDebit.Debit,
                            Amount      = amount,
                            Description = "Cybersource Deposit",
                            ObjectCode  = ObjectCodes.Income,
                        };
                        transaction.Transfers.Add(clearing);

                        // move money into holding
                        var holding = new Transfer()
                        {
                            Chart       = "3",
                            Account     = integration.HoldingAccount,
                            Direction   = Transfer.CreditDebit.Credit,
                            Amount      = amount,
                            Description = "Cybersource Deposit",
                            ObjectCode  = ObjectCodes.Income,
                        };
                        transaction.Transfers.Add(holding);

                        var errors = _context.ValidateModel(transaction);
                        if (errors.Any())
                        {
                            log.ForContext("errors", errors).Warning("Validation Errors Detected");
                            throw new Exception("Validation Errors Detected");
                        }

                        _context.Transactions.Add(transaction);

                        // push webhook for this reconcile
                        try
                        {
                            await _webHookService.SendBankReconcileWebHook(integration.Team,
                                new BankReconcileWebHookPayload()
                                {
                                    KfsTrackingNumber       = kfsTrackingNumber,
                                    MerchantTrackingNumber  = deposit.MerchantReferenceNumber,
                                    ProcessorTrackingNumber = deposit.RequestID,
                                    TransactionDate         = deposit.LocalizedRequestDate,
                                });
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error when pushing webhook information");
                        }
                    }

                    // push changes for this integration
                    var inserted = await _context.SaveChangesAsync();
                    tran.Commit();
                    log.Information("{count} records created.", new {count = inserted});
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    tran.Rollback();
                }
            }

            try
            {
                // save copy to blob storage
                var filename = $"{report.Name}.{report.OrganizationID}.{DateTime.UtcNow:yyyyMMddHHmmssffff}.xml";
                log.ForContext("container", _options.ReportBlobContainer)
                    .Information("Uploading {filename} to Blob Storage", filename);
                await using var memoryStream = new MemoryStream();
                var writer = new StreamWriter(memoryStream); // lgtm [cs/local-not-disposed]
                await writer.WriteAsync(reportXml);
                await writer.FlushAsync();
                memoryStream.Position = 0;
                await _storageService.PutBlobAsync(memoryStream, _options.ReportBlobContainer, filename);
                //TODO: store uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                log.ForContext("container", _options.ReportBlobContainer)
                    .Error(ex, ex.Message);
            }
        }
    }
}
