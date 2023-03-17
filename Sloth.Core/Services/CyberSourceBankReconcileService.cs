using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
using Sloth.Core.Abstractions;
using System.Text;

namespace Sloth.Core.Services
{
    public interface ICyberSourceBankReconcileService
    {
        Task<CybersourceBankReconcileIntegrationDetails> ProcessIntegration(Integration integration, DateTime date, ILogger log = null);

        Task<CybersourceBankReconcileIntegrationDetails> ProcessOneTimeIntegration(Integration integration, string reportName, DateTime date,
            ILogger log = null);
    }

    public class CybersourceBankReconcileDetails : IHasTransactionIds
    {
        public List<CybersourceBankReconcileIntegrationDetails> IntegrationDetails { get; set; } = new();
        public string Message { get; set; }

        public IEnumerable<string> GetTransactionIds()
        {
            return IntegrationDetails.SelectMany(i => i?.TransactionIds ?? Enumerable.Empty<string>());
        }
    }

    public class CybersourceBankReconcileIntegrationDetails
    {
        public string IntegrationId { get; set; }
        public string TeamName { get; set; }
        public string Message { get; set; }
        public string BlobId { get; set; }
        public List<string> TransactionIds { get; set; } = new();
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

        public async Task<CybersourceBankReconcileIntegrationDetails> ProcessIntegration(Integration integration, DateTime date,
            ILogger log = null)
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
                return new CybersourceBankReconcileIntegrationDetails
                {
                    IntegrationId = integration.Id,
                    TeamName = integration.Team.Name,
                    Message = "No records found."
                };
            }

            return await ProcessReport(report, reportXml, integration, log);
        }

        public async Task<CybersourceBankReconcileIntegrationDetails> ProcessOneTimeIntegration(Integration integration, string reportName, DateTime date,
            ILogger log = null)
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
                return new CybersourceBankReconcileIntegrationDetails
                {
                    IntegrationId = integration.Id,
                    TeamName = integration.Team.Name,
                    Message = "No records found."
                };
            }

            return await ProcessReport(report, reportXml, integration, log);
        }

        private async Task<CybersourceBankReconcileIntegrationDetails> ProcessReport(Report report, string reportXml, Integration integration,
            ILogger log)
        {
            var details = new CybersourceBankReconcileIntegrationDetails
            {
                IntegrationId = integration.Id,
                TeamName = integration.Team.Name,
            };
            var messageBuilder = new StringBuilder();
            var transactions = new List<Transaction>();
            var mapDepositsToDocAndKfsTrackingNumbers = new Dictionary<ReportRequest, (string docNumber, string kfsNumber)>();
            var webhookPayloads = new Dictionary<string, BankReconcileWebHookPayload>();


            await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var tran = await _context.Database.BeginTransactionAsync();
                try
                {
                    // iterate over deposits
                    foreach (var deposit in report.Requests?.ToList() ?? new List<ReportRequest>())
                    {
                        // look for existing transaction
                        var transaction =
                            await _context.Transactions.FirstOrDefaultAsync(t =>
                                t.ProcessorTrackingNumber == deposit.RequestID);
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

                        // ensure document and tracking numbers are created only once per deposit, in case this code gets
                        // called multiple times due to a transient error
                        if (!mapDepositsToDocAndKfsTrackingNumbers.TryGetValue(deposit, out var docAndkfsTrackingNumbers))
                        {
                            docAndkfsTrackingNumbers = (
                                await _context.GetNextDocumentNumber(tran.GetDbTransaction()),
                                await _context.GetNextKfsTrackingNumber(tran.GetDbTransaction())
                            );
                            mapDepositsToDocAndKfsTrackingNumbers.Add(deposit, docAndkfsTrackingNumbers);
                        }

                        transaction = new Transaction()
                        {
                            Source = integration.Source,
                            DocumentNumber = docAndkfsTrackingNumbers.docNumber,
                            KfsTrackingNumber = docAndkfsTrackingNumbers.kfsNumber,
                            MerchantTrackingNumber = deposit.MerchantReferenceNumber,
                            ProcessorTrackingNumber = deposit.RequestID,
                            TransactionDate = deposit.LocalizedRequestDate,
                            Description = "Cybersource Deposit"

                        }.SetStatus(TransactionStatuses.Scheduled);
                        transactions.Add(transaction);

                        if (AccountValidationService.IsKfsAccount(integration.ClearingAccount))
                        {
                            // move money out of clearing
                            var clearing = new Transfer()
                            {
                                Chart = "3",
                                Account = integration.ClearingAccount,
                                Direction = Transfer.CreditDebit.Debit,
                                Amount = amount,
                                Description = "Cybersource Deposit",
                                ObjectCode = ObjectCodes.Income,
                            };
                            transaction.Transfers.Add(clearing);

                            // move money into holding
                            var holding = new Transfer()
                            {
                                Chart = "3",
                                Account = integration.HoldingAccount,
                                Direction = Transfer.CreditDebit.Credit,
                                Amount = amount,
                                Description = "Cybersource Deposit",
                                ObjectCode = ObjectCodes.Income,
                            };
                            transaction.Transfers.Add(holding);
                        }
                        else
                        {
                            // move money out of clearing (Aggie Enterprise Financial String)
                            var clearing = new Transfer()
                            {
                                FinancialSegmentString = integration.ClearingAccount,
                                Direction = Transfer.CreditDebit.Debit,
                                Amount = amount,
                                Description = "Cybersource Deposit",
                            };
                            transaction.Transfers.Add(clearing);

                            // move money into holding
                            var holding = new Transfer()
                            {
                                FinancialSegmentString = integration.HoldingAccount,
                                Direction = Transfer.CreditDebit.Credit,
                                Amount = amount,
                                Description = "Cybersource Deposit",
                            };
                            transaction.Transfers.Add(holding);
                        }



                        var errors = _context.ValidateModel(transaction);
                        if (errors.Any())
                        {
                            log.ForContext("errors", errors).Warning("Validation Errors Detected");
                            throw new Exception("Validation Errors Detected");
                        }

                        _context.Transactions.Add(transaction);

                        // prepare webhook payloads for sending outside of ResilientTransaction delegate
                        if (!webhookPayloads.ContainsKey(docAndkfsTrackingNumbers.kfsNumber))
                        {
                            webhookPayloads.Add(docAndkfsTrackingNumbers.kfsNumber, new BankReconcileWebHookPayload()
                            {
                                KfsTrackingNumber = docAndkfsTrackingNumbers.kfsNumber,
                                MerchantTrackingNumber = deposit.MerchantReferenceNumber,
                                ProcessorTrackingNumber = deposit.RequestID,
                                TransactionDate = deposit.LocalizedRequestDate,
                            });
                        }
                    }

                    // push changes for this integration
                    var inserted = await _context.SaveChangesAsync();
                    await tran.CommitAsync();
                    log.Information("{transactions} transactions created.", transactions.Count);
                    messageBuilder.AppendLine($"{transactions.Count} transactions created.");
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    await tran.RollbackAsync();
                    messageBuilder.AppendLine($"Error processing report: {ex.Message}");
                }
            });

            foreach (var payload in webhookPayloads.Values)
            {
                try
                {
                    await _webHookService.SendWebHooksForTeam(integration.Team, payload);
                }
                catch (Exception ex)
                {
                    log.Error(ex, $"Error when pushing {nameof(BankReconcileWebHookPayload)}");
                    messageBuilder.AppendLine($"Error when pushing {nameof(BankReconcileWebHookPayload)}: {ex.Message}");
                }
            }

            TransactionBlob transactionBlob = null;

            try
            {
                // save copy to blob storage
                var filename = $"{report.Name}.{report.OrganizationID}.{DateTime.UtcNow:yyyyMMddHHmmssffff}.xml";
                log.ForContext("container", _options.ReportBlobContainer)
                    .Information("Uploading {filename} to Blob Storage", filename);
                await using var memoryStream = new MemoryStream();
                await using var writer = new StreamWriter(memoryStream);
                await writer.WriteAsync(reportXml);
                await writer.FlushAsync();
                memoryStream.Position = 0;
                var blob = await _storageService.PutBlobAsync(memoryStream, _options.ReportBlobContainer, filename,
                    "Cybersource Bank Reconcile Report",
                    MediaTypeNames.Application.Xml);
                transactionBlob = new TransactionBlob
                {
                    IntegrationId = integration.Id,
                    BlobId = blob.Id,
                    Blob = blob,
                };
                _context.TransactionBlobs.Add(transactionBlob);
                await _context.SaveChangesAsync();
                details.BlobId = blob.Id;
                details.TransactionIds = transactions.Select(t => t.Id).ToList();
                messageBuilder.AppendLine($"Report saved to blob storage: {blob.Id}");
            }
            catch (Exception ex)
            {
                log.ForContext("container", _options.ReportBlobContainer)
                    .Error(ex, ex.Message);
                messageBuilder.AppendLine($"Error saving report to blob storage: {ex.Message}");
            }

            details.Message = messageBuilder.ToString();

            return details;
        }
    }
}
