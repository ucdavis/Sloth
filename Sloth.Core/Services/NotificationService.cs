
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;
using Sloth.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Services
{
    public interface INotificationService
    {
        Task SendSampleNotificationMessage(string email, string body);
        Task SendNotification(string[] emails, string[] ccEmails, string body, string textVersion, string subject = "Harvest Notification");
    }

    public class NotificationService : INotificationService
    {
        private readonly SmtpClient _client;
        private readonly SlothDbContext _dbContext;
        private readonly SparkpostOptions _emailSettings;

        public NotificationService(SlothDbContext dbContext, IOptions<SparkpostOptions> emailSettings)
        {
            _dbContext = dbContext;
            _emailSettings = emailSettings.Value;
            _client = new SmtpClient(_emailSettings.Host, _emailSettings.Port) { Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password), EnableSsl = true };
        }
        public async Task SendSampleNotificationMessage(string email, string body)
        {
            if(_emailSettings.DisableSend.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            using (var message = new MailMessage { From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName), Subject = "Harvest Notification" })
            {
                message.To.Add(new MailAddress(email, email));

                // body is our fallback text and we'll add an HTML view as an alternate.
                message.Body = "Sample Email Text";

                var htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType(MediaTypeNames.Text.Html));
                message.AlternateViews.Add(htmlView);

                await _client.SendMailAsync(message);
            }
        }

        public async Task SendNotification(string[] emails, string[] ccEmails, string body, string textVersion, string subject = "Harvest Notification")
        {
            if (_emailSettings.DisableSend.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            using (var message = new MailMessage { From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName), Subject = subject })
            {
                foreach (var email in emails)
                {
                    message.To.Add(new MailAddress(email, email));
                }

                if (ccEmails != null && ccEmails.Length > 0)
                {
                    foreach (var ccEmail in ccEmails)
                    {
                        message.CC.Add(new MailAddress(ccEmail));
                    }
                }

                if (!string.IsNullOrWhiteSpace(_emailSettings.BccEmail))
                {
                    message.Bcc.Add(new MailAddress(_emailSettings.BccEmail));
                }

                // body is our fallback text and we'll add an HTML view as an alternate.
                message.Body = textVersion;

                var htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType(MediaTypeNames.Text.Html));
                message.AlternateViews.Add(htmlView);

                await _client.SendMailAsync(message);
            }
        }
    }
}
