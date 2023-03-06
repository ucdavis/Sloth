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

            // get transactions that are rejected or have been processing for longer than 5 days
            var teamsWithFailedTransactions = await _dbContext.Transactions
                .Where(t => t.Status == TransactionStatuses.Rejected
                    || (t.Status == TransactionStatuses.Processing && t.LastModified < fiveDaysAgo))
                .Select(t => t.Source.Team.Slug)
                .Distinct()
                .ToArrayAsync();

            var failedTxnResult = new FailedTxnResult();

            if (!teamsWithFailedTransactions.Any())
            {
                Log.Information("No failed transactions found");
                return failedTxnResult;
            }

            foreach (var team in teamsWithFailedTransactions)
            {
                var notifySuccess = await _notificationService.Notify(Notification
                    .Message($"One or more {team} transactions have failed")
                    .WithEmailsToTeam(team, TeamRole.Admin)
                    .WithCcEmailsToTeam(team, TeamRole.Manager)
                    .WithLinkBack("View Failed Transactions", $"/{team}/Reports/FailedTransactions"));
                if (!notifySuccess)
                {
                    Log.Error("Error sending failed transactions notification for team {TeamSlug}", team);
                    failedTxnResult.FailedTxnTeamsNotEmailed.Add(team);
                    //TODO: queue notification for retry
                }
                else
                {
                    failedTxnResult.FailedTxnTeamsEmailed.Add(team);
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