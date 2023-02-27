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
    public class NotificationJob
    {
        private readonly INotificationService _notificationService;
        private readonly SlothDbContext _dbContext;

        public NotificationJob(INotificationService notificationService, SlothDbContext dbContext)
        {
            _notificationService = notificationService;
            _dbContext = dbContext;
        }

        public async Task ProcessNotifications()
        {
            await ProcessFailedTransactionNotifications();
        }

        private async Task ProcessFailedTransactionNotifications()
        {
            var fiveDaysAgo = DateTime.UtcNow.Date.AddDays(-5);
            var sixDaysAgo = DateTime.UtcNow.Date.AddDays(-6);

            var groups = await _dbContext.Transactions
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Where(t =>
                    t.Status == TransactionStatuses.Rejected
                    || (
                        t.Status == TransactionStatuses.Processing
                        && (t.StatusEvents.Where(e => e.Status == TransactionStatuses.Processing)
                            .Max(e => (DateTime?)e.EventDate) ?? sixDaysAgo) < fiveDaysAgo
                            // casting to nullable is an MS-condoned hack to allow calling Max on a potentially empty collection
                    )
                )
                .GroupBy(t => t.Source.Team.Slug)
                .ToArrayAsync();

            if (!groups.Any())
            {
                Log.Information("No failed transactions found");
                return;
            }

            foreach (var group in groups)
            {
                var notifySuccess = await _notificationService.Notify(Notification
                    .Message("One or more transactions have failed")
                    .WithEmailsToTeam(group.Key, TeamRole.Admin)
                    .WithCcEmailsToTeam(group.Key, TeamRole.Manager)
                    .WithLinkBack("View Failed Transactions", $"/{group.Key}/Reports/FailedTransactions"));
                if (!notifySuccess)
                {
                    Log.Error("Error sending failed transactions notification for team {TeamSlug}", group.Key);
                    //TODO: queue notification for retry
                }
            }
        }
    }
}
