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

        public Task ProcessNotifications(ILogger log)
        {
            return ProcessFailedTransactionNotifications(log);
        }

        private async Task ProcessFailedTransactionNotifications(ILogger log)
        {
            var groups = await _dbContext.Transactions
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Where(t =>
                    (
                        t.Status == TransactionStatuses.Processing
                        && t.StatusEvents.Where(e => e.Status == TransactionStatuses.Processing).Max(e => e.EventDate) < DateTime.UtcNow.Date.AddDays(-5)
                        && t.StatusEvents.Where(e => e.Status == TransactionStatuses.NotificationSent).Max(e => e.EventDate) < DateTime.UtcNow.Date.AddDays(-5)
                    )
                    ||
                    (
                        (t.Status == TransactionStatuses.Processing && t.StatusEvents.Where(e => e.Status == TransactionStatuses.NotificationSent).Max(e => e.EventDate) < DateTime.UtcNow.Date.AddDays(-5))
                    )
                )
                .GroupBy(t => t.Source.Team.Slug)
                .ToArrayAsync();

            foreach (var group in groups)
            {


                var notifySuccess = await _notificationService.Notify(Notification
                    .Message("One or more transactions have failed")
                    .WithEmailsToTeam(group.Key, TeamRole.Admin)
                    .WithCcEmailsToTeam(group.Key, TeamRole.Manager)
                    .WithLinkBack("View Failed Transactions", $"/{group.Key}/Reports/FailedTransactions"));
                if (!notifySuccess)
                {
                    log.Error("Error sending failed transactions notification for team {TeamSlug}", group.Key);
                    //TODO: queue notification for retry
                }
            }
        }
    }
}
