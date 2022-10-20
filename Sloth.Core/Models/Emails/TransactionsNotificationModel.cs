namespace Sloth.Core.Models.Emails
{
    public class TransactionsNotificationModel
    {
        public string Title { get; set; }
        public string NotificationText { get; set; }

        public string ButtonUrl1 { get; set; }
        public string ButtonText1 { get; set; } = "View Transactions";
    }
}
