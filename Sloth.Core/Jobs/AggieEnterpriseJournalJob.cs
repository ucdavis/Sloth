using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core.Resources;
using Sloth.Core.Services;

namespace Sloth.Core.Jobs
{
    public class AggieEnterpriseJournalJob
    {
        private readonly SlothDbContext _context;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public static string JobName = "AggieEnterprise.JournalProcessor";

        public AggieEnterpriseJournalJob(SlothDbContext context, IAggieEnterpriseService aggieEnterpriseService)
        {
            _context = context;
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        public async Task UploadTransactions(ILogger log)
        {
            try
            {
                // fetch all staged transactions
                var transactions = await _context.Transactions
                    .Where(t => t.Status == TransactionStatuses.Scheduled)
                    .Include(t => t.Transfers)
                    .Include(t => t.Source)
                        .ThenInclude(s => s.Team)
                    .Include(t => t.ReversalOfTransaction)
                    .ToListAsync();

                if (!transactions.Any())
                {
                    log.Information("No scheduled transactions found.");
                    return;
                }

                // group transactions by origin code and feed
                var groups = transactions.GroupBy(t => t.Source);
                foreach (var group in groups)
                {
                    try
                    {
                        var source = group.Key;
                        var groupedTransactions = group.ToList();

                        var originCode = source.OriginCode;

                        // TODO:
                        // loop through grouped transactions and upload to Aggie Enterprise, saving status of each
                        
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, ex.Message);
                        log.Error($"Aggie Enterprise Upload error for source {group.Key.Name}");
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
