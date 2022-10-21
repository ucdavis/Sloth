using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Harvest.Core.Services;
using Razor.Templating.Core;
using Renci.SshNet.Messages;
using Serilog;

namespace Sloth.Core.Services
{
    public interface INotificationService
    {
        Task<bool> Notify(Notification nodification);
    }

    public class NotificationService : INotificationService
    {
        private readonly ISmtpService _smtpService;
        public NotificationService(ISmtpService smtpService)
        {
            _smtpService = smtpService;
        }

        public async Task<bool> Notify(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (notification.Emails == null || notification.Emails.Length == 0)
            {
                throw new ArgumentException("No email addresses specified", nameof(notification.Emails));
            }

            if ((string.IsNullOrWhiteSpace(notification.Body) && string.IsNullOrWhiteSpace(notification.ViewName))
                || (!string.IsNullOrWhiteSpace(notification.Body) && !string.IsNullOrWhiteSpace(notification.ViewName)))
            {
                throw new ArgumentException("Either Body or ViewName must be specified, but not both", nameof(notification.Body) + " or " + nameof(notification.ViewName));
            }

            try
            {
                var body = !string.IsNullOrWhiteSpace(notification.Body)
                    ? notification.Body
                    : await RazorTemplateEngine.RenderAsync(notification.ViewName, notification.Model, notification.ViewBagOrViewData);

                await _smtpService.SendEmail(notification.Emails, notification.CcEmails, body, notification.TextVersion, notification.Subject);
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
        public string[] Emails { get; set; }
        public string[] CcEmails { get; set; } = new string[] { };
        public string TextVersion { get; set; } = "";

        public string Body { get; set; }

        // remaining properties are mutually exclusive with Body
        public string ViewName { get; set; }
        public object Model { get; set; }
        public Dictionary<string, object> ViewBagOrViewData = new Dictionary<string, object>();

        public static Notification Message(string message, string[] emails)
        {
            return new Notification
            {
                Emails = emails,
                Body = message,
                TextVersion = message,
            };
        }

        public static Notification View(string viewName, string[] emails)
        {
            return new Notification
            {
                ViewName = viewName,
                Emails = emails
            };
        }

        public Notification WithTextVersion(string textVersion)
        {
            TextVersion = textVersion;
            return this;
        }

        public Notification WithSubject(string subject)
        {
            Subject = subject;
            return this;
        }

        public Notification WithCcEmails(string[] ccEmails)
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
    }
}
