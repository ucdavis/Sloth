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

        /// <summary>
        /// Process Reconcile
        /// </summary>
        /// <param name="date">Date of reoncile</param>
        /// <param name="log">logger</param>
        /// <param name="jobRecord">Job record to be linked to any transactions created by this operation, or null if reconcile
        /// was manually launched</param>
        /// <returns></returns>
        public async Task ProcessReconcile(DateTime date, ILogger log, CybersourceBankReconcileJobRecord jobRecord = null)
        {
            log = log.ForContext("date", date);

            try
            {
                var integrations = _context.Integrations
                    .Where(i => i.Type == IntegrationTypes.CyberSource)
                    .Include(i => i.Source)
                    .Include(i => i.Team)
                    .ToList();

                if (!integrations.Any())
                {
                    log.Information("Early exit, no active integrations found.");
                }

                foreach (var integration in integrations)
                {
                    try
                    {
                        await _cyberSourceBankReconcileService.ProcessIntegration(integration, date, log, jobRecord);
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
