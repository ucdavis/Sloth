using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core.Domain;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Core.Jobs
{
    public class CybersourceBankReconcileJob
    {
        private readonly SlothDbContext _context;
        private readonly ICyberSourceBankReconcileService _cyberSourceBankReconcileService;

        public static string JobName = "Cybersource.BankReconcile";

        public CybersourceBankReconcileJob(SlothDbContext context, ICyberSourceBankReconcileService cyberSourceBankReconcileService)
        {
            _context = context;
            _cyberSourceBankReconcileService = cyberSourceBankReconcileService;
        }

        public async IAsyncEnumerable<CybersourceBankReconcileJobBlob> ProcessReconcile(DateTime date,
            CybersourceBankReconcileJobRecord jobRecord, ILogger log)
        {
            log = log.ForContext("date", date);

            List<Integration> integrations;

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
                    yield break;
                }

            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                yield break;
            }

            foreach (var integration in integrations)
            {
                var innerLog = log.ForContext("TeamName", integration.Team.Name);
                innerLog.Information("Starting integration for {TeamName}");
                CybersourceBankReconcileJobBlob jobBlob = null;
                try
                {
                    jobBlob = await _cyberSourceBankReconcileService.ProcessIntegration(integration, date, jobRecord, innerLog);                    
                }
                catch (Exception ex)
                {
                    innerLog.Error(ex, ex.Message);
                }
                innerLog.Information("Completed integration for {TeamName}");

                if (jobBlob != null)
                    yield return jobBlob;
            }
        }
    }
}
