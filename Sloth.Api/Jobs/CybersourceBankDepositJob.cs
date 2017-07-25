using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Sloth.Api.Services;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Integrations.Cybersource;
using Sloth.Integrations.Cybersource.Clients;

namespace Sloth.Api.Jobs
{
    public class CybersourceBankDepositJob : JobBase
    {
        private readonly SlothDbContext _context;
        private readonly ISecretsService _secretsService;
        private readonly CybersourceSettings _cybersourceSettings;

        public CybersourceBankDepositJob(SlothDbContext context, IConfiguration configuration, ISecretsService secretsService)
        {
            _context = context;
            _secretsService = secretsService;

            _cybersourceSettings = new CybersourceSettings();
            configuration.GetSection("Cybersource").Bind(_cybersourceSettings);
        }

        [RecurringJob(CronStrings.Hourly, RecurringJobId = "cybersource-bank-deposit")]
        public async Task UploadScrubber(PerformContext context)
        {
            SetupLogging(context);

            var log = Logger.ForContext("jobId", context.BackgroundJob.Id);

            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            try
            {
                var integrations = _context.Integrations.Where(i => i.Type == Integration.IntegrationType.CyberSource);

                foreach (var integration in integrations)
                {
                    // fetch password secret
                    var password = await _secretsService.GetSecret(integration.ReportPasswordKey);

                    // create client
                    var client = new ReportClient(_cybersourceSettings.ReportUrl, integration.MerchantId, integration.ReportUsername, password);

                    // fetch report
                    var report = await client.GetPaymentBatchDetailReport(yesterday);
                    var count = report.Batches?.Batch?.Length ?? 0;

                    log.Information("Report found with {count} records.", new { count });
                    if (count < 1)
                    {
                        continue;
                    }

                    // iterate over deposits
                    foreach (var deposit in report.Batches?.Batch?.SelectMany(b => b.Requests?.Request) ?? new List<Request>())
                    {
                        // create transaction per deposit item,
                        // moving monies from clearing to holding account
                        var transaction = new Transaction()
                        {
                            Status = TransactionStatus.Scheduled
                        };

                        var clearing = new Transfer()
                        {
                            Account        = _cybersourceSettings.ClearingAccount,
                            Direction      = Transfer.CreditDebit.Debit,
                            Amount         = deposit.Amount,
                            TrackingNumber = deposit.MerchantReferenceNumber,
                        };
                        transaction.Transfers.Add(clearing);

                        var holding = new Transfer()
                        {
                            Account        = _cybersourceSettings.HoldingAccount,
                            Direction      = Transfer.CreditDebit.Credit,
                            Amount         = deposit.Amount,
                            TrackingNumber = deposit.MerchantReferenceNumber
                        };
                        transaction.Transfers.Add(holding);

                        // then from holding to the default account
                        var holding2 = new Transfer()
                        {
                            Account        = _cybersourceSettings.HoldingAccount,
                            Direction      = Transfer.CreditDebit.Debit,
                            Amount         = deposit.Amount,
                            TrackingNumber = deposit.MerchantReferenceNumber
                        };
                        transaction.Transfers.Add(holding2);

                        var final = new Transfer()
                        {
                            Account        = integration.DefaultAccount,
                            Direction      = Transfer.CreditDebit.Credit,
                            Amount         = deposit.Amount,
                            TrackingNumber = deposit.MerchantReferenceNumber
                        };
                        transaction.Transfers.Add(final);

                        _context.Transactions.Add(transaction);
                    }

                    // push changes for this integration
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                throw;
            }
        }

        public class CybersourceSettings
        {
            public string ReportUrl { get; set; }

            public string ClearingAccount { get; set; }

            public string HoldingAccount { get; set; }
        }
    }
}
