using System;
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

        public static string JobName = "Cybersource.BankReconcile";

        public CybersourceBankReconcileJob(SlothDbContext context, ICyberSourceBankReconcileService cyberSourceBankReconcileService)
        {
            _context = context;
            _cyberSourceBankReconcileService = cyberSourceBankReconcileService;
        }

        public async Task ProcessReconcile(DateTime date, CybersourceBankReconcileJobRecord jobRecord, ILogger log)
        {
            log = log.ForContext("date", date);

            try
            {
                var integrations = await _context.Integrations
                    .Where(i => i.Type == IntegrationTypes.CyberSource)
                    .Include(i => i.Source)
                    .Include(i => i.Team)
                    .ToListAsync();

                if (!integrations.Any())
                {
                    log.Information("Early exit, no active integrations found.");
                }

                foreach (var integration in integrations)
                {
                    try
                    {
                        await _cyberSourceBankReconcileService.ProcessIntegration(integration, date, jobRecord, log);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, ex.Message);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
            }
        }
    }
}
