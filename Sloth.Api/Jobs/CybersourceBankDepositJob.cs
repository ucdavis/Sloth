using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Sloth.Api.Services;
using Sloth.Core;
using Sloth.Core.Models;
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

                    log.Information("Report found with {count} records.", new { count = report.Batches?.Batch?.Length ?? 0 });
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
        }
    }
}
