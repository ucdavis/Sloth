using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Harvest.Core.Services;
using Razor.Templating.Core;
using Renci.SshNet.Messages;
using Serilog;
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

            if (string.IsNullOrWhiteSpace(notification.Body)
                && string.IsNullOrWhiteSpace(notification.ViewName)
                && string.IsNullOrWhiteSpace(notification.MessageText))
            {
                throw new ArgumentException("Either Body, ViewName or MessageText must be specified",
                    $"{nameof(notification.Body)} or {nameof(notification.ViewName)} or {nameof(notification.MessageText)}");
            }

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
                            });

                await _smtpService.SendEmail(notification.Emails, notification.CcEmails, body, notification.MessageText, notification.Subject);
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
                Emails = emails,
                MessageText = message,
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

        public Notification WithTextVersion(string textVersion)
        {
            MessageText = textVersion;
            return this;
        }

        public Notification WithSubject(string subject)
        {
            Subject = subject;
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

        public Notification WithLinkBack(string text, string url)
        {
            LinkBackButton = new EmailButtonModel
            {
                Url = url,
                Text = text
            };
            return this;
        }
    }
}
