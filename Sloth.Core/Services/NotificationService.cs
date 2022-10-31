using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sloth.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Razor.Templating.Core;
using Renci.SshNet.Messages;
using Serilog;
using Sloth.Core.Configuration;
using Sloth.Core.Models.Emails;
using Sloth.Core.Views.Shared;

namespace Sloth.Core.Services
{
    public interface INotificationService
    {
        Task<bool> Notify(Notification nodification);
    }

    public class NotificationService : INotificationService
    {
        private readonly NotificationOptions _notificationOptions;
        private readonly ISmtpService _smtpService;
        private readonly SlothDbContext _dbContext;

        public NotificationService(IOptions<NotificationOptions> notificationOptions,ISmtpService smtpService, SlothDbContext dbContext)
        {
            _notificationOptions = notificationOptions.Value;
            _smtpService = smtpService;
            _dbContext = dbContext;
        }

        public async Task<bool> Notify(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (notification.TeamRolesForEmails != null)
            {
                // get email addresses based on team slug and roles
                notification.Emails = await _dbContext.Users
                    .Where(u => u.UserTeamRoles.Any(utr =>
                        utr.Team.Slug == notification.TeamRolesForEmails.Slug
                        && (notification.TeamRolesForEmails.Roles.Contains(utr.Role.Name))))
                    .Select(u => u.Email)
                    .ToArrayAsync();
            }

            if (notification.TeamRolesForCcEmails != null)
            {
                // get cc email addresses based on team slug and roles
                notification.CcEmails = await _dbContext.Users
                    .Where(u => u.UserTeamRoles.Any(utr =>
                        utr.Team.Slug == notification.TeamRolesForCcEmails.Slug
                        && (notification.TeamRolesForCcEmails.Roles.Contains(utr.Role.Name))))
                    .Select(u => u.Email)
                    .ToArrayAsync();
            }

            if (notification.Emails == null || !notification.Emails.Any())
            {
                throw new ArgumentException("No email addresses specified", nameof(notification.Emails));
            }

            if (notification.CcEmails == null)
            {
                notification.CcEmails = Enumerable.Empty<string>();
            }

            if (string.IsNullOrWhiteSpace(notification.Body)
                && string.IsNullOrWhiteSpace(notification.ViewName)
                && string.IsNullOrWhiteSpace(notification.MessageText))
            {
                throw new ArgumentException("Either Body, ViewName or MessageText must be specified",
                    $"{nameof(notification.Body)} or {nameof(notification.ViewName)} or {nameof(notification.MessageText)}");
            }

            if (notification.ViewBagOrViewData == null)
            {
                notification.ViewBagOrViewData = new Dictionary<string, object>();
            }

            notification.ViewBagOrViewData.Add("LinkBackBaseUrl", _notificationOptions.LinkBackBaseUrl);

            try
            {
                var body = !string.IsNullOrWhiteSpace(notification.Body)
                    ? notification.Body
                    : !string.IsNullOrWhiteSpace(notification.ViewName)
                        ? await RazorTemplateEngine.RenderAsync(notification.ViewName, notification.Model, notification.ViewBagOrViewData)
                        : await RazorTemplateEngine.RenderAsync("/Views/Emails/DefaultNotification.cshtml", new DefaultNotificationModel
                        {
                            MessageText = notification.MessageText,
                            Subject = notification.Subject,
                            LinkBackButton = notification.LinkBackButton
                        }, notification.ViewBagOrViewData);

                await _smtpService.SendEmail(notification.Emails.ToArray(), notification.CcEmails.ToArray(), body, notification.MessageText, notification.Subject);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error sending notification for subject \"{Subject}\"", notification.Subject);
                return false;
            }
            return true;
        }
    }

    public class Notification
    {
        public string Subject { get; set; } = "Sloth Notification";
        public IEnumerable<string> Emails { get; set; }
        public IEnumerable<string> CcEmails { get; set; }

        // TeamRoles can be specified to let the service fill in Emails and CcEmails
        public TeamRoles TeamRolesForEmails { get; set; }
        public TeamRoles TeamRolesForCcEmails { get; set; }

        // Used only by DefaultNotification view. If using a custom view and model, a property of
        // this type and name in your model will be picked up by the _EmailLayout_mjml view
        public EmailButtonModel LinkBackButton { get; set; }

        // Used as alternativeText and as content in DefaultNotification view
        public string MessageText { get; set; } = "";

        // If set, overrides usage of DefaultNotification view
        public string ViewName { get; set; }
        public object Model { get; set; }
        public Dictionary<string, object> ViewBagOrViewData = new Dictionary<string, object>();

        // If set, overrides all usage of view templates
        public string Body { get; set; }

        public static Notification Message(string message, params string[] emails)
        {
            return new Notification
            {
                MessageText = message,
                Emails = emails
            };
        }

        public static Notification View(string viewName, params string[] emails)
        {
            return new Notification
            {
                ViewName = viewName,
                Emails = emails
            };
        }

        public Notification WithEmailsToTeam(string slug, string role, params string[] additionalRoles)
        {
            TeamRolesForEmails = new TeamRoles
            {
                Slug = slug,
                Roles = Enumerable.Repeat(role, 1).Concat(additionalRoles)
            };
            return this;
        }

        public Notification WithCcEmailsToTeam(string slug, string role, params string[] additionalRoles)
        {
            TeamRolesForCcEmails = new TeamRoles
            {
                Slug = slug,
                Roles = Enumerable.Repeat(role, 1).Concat(additionalRoles)
            };

            return this;
        }

        public Notification WithSubject(string subject)
        {
            Subject = subject;
            return this;
        }

        public Notification WithEmails(params string[] emails)
        {
            Emails = emails;
            return this;
        }

        public Notification WithCcEmails(params string[] ccEmails)
        {
            CcEmails = ccEmails;
            return this;
        }

        public Notification WithModel(object model)
        {
            Model = model;
            return this;
        }

        public Notification WithData(Dictionary<string, object> viewBagOrViewData)
        {
            ViewBagOrViewData = viewBagOrViewData;
            return this;
        }

        public Notification WithLinkBack(string text, string urlPath = "")
        {
            LinkBackButton = new EmailButtonModel
            {
                UrlPath = urlPath,
                Text = text
            };
            return this;
        }

        public class TeamRoles
        {
            public string Slug { get; set; }
            public IEnumerable<string> Roles { get; set; }
        }
    }
}
