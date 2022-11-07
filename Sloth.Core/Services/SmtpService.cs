
using Sloth.Core.Models.Settings;
using Microsoft.Extensions.Options;
using Sloth.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Sloth.Core.Services
{
    public interface ISmtpService
    {
        Task SendSampleEmail(string email, string body);
        Task SendEmail(string[] emails, string[] ccEmails, string body, string textVersion, string subject = "Sloth Notification");
    }

    public class SmtpService : ISmtpService
    {
        private readonly SmtpClient _client;
        private readonly SparkpostOptions _emailSettings;

        public SmtpService(SlothDbContext dbContext, IOptions<SparkpostOptions> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            _client = new SmtpClient(_emailSettings.Host, _emailSettings.Port) { Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password), EnableSsl = true };
        }
        public async Task SendSampleEmail(string email, string body)
        {
            if(_emailSettings.DisableSend.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            using (var message = new MailMessage { From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName), Subject = "Sloth Notification" })
            {
                message.To.Add(new MailAddress(email, email));

                // body is our fallback text and we'll add an HTML view as an alternate.
                message.Body = "Sample Email Text";

                var htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType(MediaTypeNames.Text.Html));
                message.AlternateViews.Add(htmlView);

                await _client.SendMailAsync(message);
            }
        }

        public async Task SendEmail(string[] emails, string[] ccEmails, string body, string textVersion, string subject = "Sloth Notification")
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
