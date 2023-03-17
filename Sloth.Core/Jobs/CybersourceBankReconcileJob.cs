using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Core.Jobs
{
    public class CybersourceBankReconcileJob
    {
        private readonly SlothDbContext _context;
        private readonly ICyberSourceBankReconcileService _cyberSourceBankReconcileService;

        public const string JobName = "Cybersource.BankReconcile";

        public CybersourceBankReconcileJob(SlothDbContext context, ICyberSourceBankReconcileService cyberSourceBankReconcileService)
        {
            _context = context;
            _cyberSourceBankReconcileService = cyberSourceBankReconcileService;
        }

        public async Task<CybersourceBankReconcileDetails> ProcessReconcile(DateTime date, ILogger log)
        {
            log = log.ForContext("date", date);

            List<Integration> integrations;
            var details = new CybersourceBankReconcileDetails();

            try
            {
                integrations = await _context.Integrations
                    .Where(i => i.Type == IntegrationTypes.CyberSource)
                    .Include(i => i.Source)
                    .Include(i => i.Team)
                    .ToListAsync();

                if (!integrations.Any())
                {
                    log.Information("Early exit, no active integrations found.");
                    details.Messages.Add("No active integrations found");
                    return details;
                }

            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                details.Messages.Add("Error fetching integrations");
                return details;
            }

            foreach (var integration in integrations)
            {
                var innerLog = log.ForContext("TeamName", integration.Team.Name);
                innerLog.Information("Starting integration for {TeamName}");
                try
                {
                    details.IntegrationDetails.Add(await _cyberSourceBankReconcileService.ProcessIntegration(integration, date, innerLog));
                }
                catch (Exception ex)
                {
                    // ProcessIntegration is pretty aggressive about swallowing and logging exceptions, but just in case...
                    innerLog.Error(ex, $"Error processing integration: {ex.Message}");
                    details.IntegrationDetails.Add(new CybersourceBankReconcileIntegrationDetails
                    {
                        IntegrationId = integration.Id,
                        TeamName = integration.Team.Name,
                        Messages = new() { $"Error processing integration: {ex.Message}" }
                    });
                }
                innerLog.Information("Completed integration for {TeamName}");
            }

            details.Messages.Add($"Processed {details.IntegrationDetails.Count} integrations");

            return details;
        }


    }
}
