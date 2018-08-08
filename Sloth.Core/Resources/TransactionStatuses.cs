using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Core.Resources
{
    public class TransactionStatuses
    {
        public const string PendingApproval = "PendingApproval";
        public const string Scheduled = "Scheduled";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static string[] GetAllStatuses()
        {
            return new[]
            {
                PendingApproval,
                Scheduled,
                Completed,
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

                case Completed:
                    return "badge-success";

                case Cancelled:
                    return "badge-danger";

                default:
                    return "badge-secondary";
            }
        }
    }
}
