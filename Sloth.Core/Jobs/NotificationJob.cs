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
            var jobDetails = new NotificationJobDetails();
            jobDetails.NotificationResults.Add(await ProcessFailedTransactionNotifications());
            jobDetails.NotificationResults.Add(await NotifyApproversAboutReversals());
            return jobDetails;
        }

        private async Task<NotificationResult> ProcessFailedTransactionNotifications()
        {
            var fiveDaysAgo = DateTime.UtcNow.Date.AddDays(-5);

            // get transactions that are rejected or have been processing for longer than 5 days
            var teamsWithFailedTransactions = await _dbContext.Transactions
                .Where(t => t.Status == TransactionStatuses.Rejected
                    || (t.Status == TransactionStatuses.Processing && t.LastModified < fiveDaysAgo))
                .Select(t => t.Source.Team.Slug)
                .Distinct()
                .ToArrayAsync();

            var failedTxnResult = new NotificationResult();

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
                    failedTxnResult.TeamsNotEmailed.Add(team);
                    //TODO: queue notification for retry
                }
                else
                {
                    failedTxnResult.TeamsEmailed.Add(team);
                }
            }

            return failedTxnResult;
        }

        private async Task<NotificationResult> NotifyApproversAboutReversals()
        {
            var teamsWithPendingReversals = await _dbContext.Transactions
                .Where(t => t.Status == TransactionStatuses.PendingApproval && !string.IsNullOrWhiteSpace(t.ReversalOfTransactionId))
                .Select(t => t.Source.Team.Slug)
                .Distinct()
                .ToArrayAsync();

            var failedTxnResult = new NotificationResult();

            if (!teamsWithPendingReversals.Any())
            {
                Log.Information("No reversals needing approval transactions found");
                return failedTxnResult;
            }

            foreach (var team in teamsWithPendingReversals)
            {
                var notifySuccess = await _notificationService.Notify(Notification
                    .Message($"One or more {team} transactions have Pending Reversals")
                    .WithEmailsToTeam(team, TeamRole.Approver)
                    .WithCcEmailsToTeam(team, TeamRole.Manager)
                    .WithLinkBack("View Transactions Needing Approval", $"/{team}/Transactions/NeedApproval"));
                if (!notifySuccess)
                {
                    Log.Error("Error sending Transactions Needing Approval notification for team {TeamSlug}", team);
                    failedTxnResult.TeamsNotEmailed.Add(team);
                    //TODO: queue notification for retry
                }
                else
                {
                    failedTxnResult.TeamsEmailed.Add(team);
                }
            }

            return failedTxnResult;
        }
    }

    public class NotificationResult
    {
        public List<string> TeamsEmailed { get; set; } = new();
        public List<string> TeamsNotEmailed { get; set; } = new();
    }

    public class NotificationJobDetails
    {
        public List<NotificationResult> NotificationResults { get; set; } = new();
    }
}
