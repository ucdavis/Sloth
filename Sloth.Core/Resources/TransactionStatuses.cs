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
                    return "badge-warning";

                case Scheduled:
                    return "badge-info";

                case Processing:
                    return "badge-info";

                case Completed:
                    return "badge-success";

                case Rejected:
                    return "badge-danger";

                case Cancelled:
                    return "badge-danger";

                default:
                    return "badge-secondary";
            }
        }
    }
}
