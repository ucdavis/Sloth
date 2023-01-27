using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Core.Resources
{
    public class TransactionStatuses
    {
        public const string PendingApproval = "PendingApproval";
        public const string Scheduled = "Scheduled";
        public const string Processing = "Processing";
        public const string Completed = "Completed";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";

        // Only used to keep track of the last time a notification was sent; not for actually setting Transaction.Status
        public const string NotificationSent = "NotificationSent";

        public static string[] GetAllStatuses()
        {
            return new[]
            {
                PendingApproval,
                Scheduled, // waiting to be uploaded
                Processing, // uploaded, but not yet processed
                Completed, // uploaded and processed
                Rejected, // upload failed
                Cancelled,
            };
        }

        public static string GetBadgeClass(string status)
        {
            switch (status)
            {
                case PendingApproval:
                    return "badge-pending";

                case Scheduled:
                    return "badge-scheduled";

                case Processing:
                    return "badge-processing";

                case Completed:
                    return "badge-completed";

                case Rejected:
                    return "badge-rejected";

                case Cancelled:
                    return "badge-cancelled";

                default:
                    return "badge-default";
            }
        }
    }
}
