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
    public class NotificationJob
    {
        public const string JobName = "Notifications";
        private readonly INotificationService _notificationService;
        private readonly SlothDbContext _dbContext;

        public NotificationJob(INotificationService notificationService, SlothDbContext dbContext)
        {
            _notificationService = notificationService;
            _dbContext = dbContext;
        }

        public async Task<NotificationJobDetails> ProcessNotifications()
        {
            var jobDetails = new NotificationJobDetails
            {
                FailedTxnResult = await ProcessFailedTransactionNotifications()
            };
            return jobDetails;
        }

        private async Task<FailedTxnResult> ProcessFailedTransactionNotifications()
        {
            var fiveDaysAgo = DateTime.UtcNow.Date.AddDays(-5);
            var sixDaysAgo = DateTime.UtcNow.Date.AddDays(-6);

            var txnIdsFromStaleProcessingEvents = await _dbContext.TransactionStatusEvents
                .Where(e => e.Status == TransactionStatuses.Processing)
                .GroupBy(e => e.TransactionId)
                .Select(g => new
                {
                    TransactionId = g.Key,
                    MaxDate = g.Max(e => (DateTime?)e.EventDate) ?? fiveDaysAgo
                })
                .Where(e => e.MaxDate > fiveDaysAgo)
                .Select(e => e.TransactionId)
                .ToArrayAsync();

            var groups = await _dbContext.Transactions
                .Include(t => t.Source)
                    .ThenInclude(s => s.Team)
                .Where(t =>
                    t.Status == TransactionStatuses.Rejected
                    //|| txnIdsFromStaleProcessingEvents.Contains(t.Id)

                    // || (
                    //     t.Status == TransactionStatuses.Processing
                    //     && (t.StatusEvents.Where(e => e.Status == TransactionStatuses.Processing)
                    //         .Max(e => (DateTime?)e.EventDate) ?? sixDaysAgo) < fiveDaysAgo
                    // // casting to nullable is an MS-condoned hack to allow calling Max on a potentially empty collection
                    // )
                )
                .GroupBy(t => t.Source.Team.Slug)
                .ToArrayAsync();

            var failedTxnResult = new FailedTxnResult();

            if (!groups.Any())
            {
                Log.Information("No failed transactions found");
                return failedTxnResult;
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
                    failedTxnResult.FailedTxnTeamsNotEmailed.Add(group.Key);
                    //TODO: queue notification for retry
                }
                else
                {
                    failedTxnResult.FailedTxnTeamsEmailed.Add(group.Key);
                }
            }

            return failedTxnResult;
        }
    }

    public class FailedTxnResult
    {
        public List<string> FailedTxnTeamsEmailed { get; set; } = new();
        public List<string> FailedTxnTeamsNotEmailed { get; set; } = new();
    }

    public class NotificationJobDetails
    {
        public FailedTxnResult FailedTxnResult { get; set; } = new();
    }
}
