using Sloth.Core.Views.Shared;

namespace Sloth.Core.Models.Emails
{
    public class DefaultNotificationModel
    {
        public string Subject { get; set; }
        public string MessageText { get; set; }
        public EmailButtonModel LinkBackButton { get; set; }
    }
}
