using Sloth.Web.Models.TransactionViewModels;
using System;

namespace Sloth.Web.Helpers
{
    public class FilterHelpers
    {
        public static void SanitizeTransactionsFilter(TransactionsFilterModel model)
        {
            var fromUtc = (model.From ?? DateTime.Now.AddMonths(-1)).ToUniversalTime().Date;
            var throughUtc = (model.To ?? DateTime.Now).ToUniversalTime().AddDays(1).Date;

            if (fromUtc > DateTime.UtcNow || fromUtc < DateTime.UtcNow.AddYears(-100))
            {
                // invalid, so default to filtering from one month ago
                var from = DateTime.Now.AddMonths((-1)).Date;
                model.From = from;
                fromUtc = from.ToUniversalTime();
            }
            else
            {
                model.From = fromUtc.ToLocalTime();
            }

            if (fromUtc >= throughUtc)
            {
                // invalid, so default to filtering through one month after fromUtc
                throughUtc = fromUtc.AddMonths(1).AddDays(1).Date;
                model.To = throughUtc.AddDays(-1).ToLocalTime();
            }
            else
            {
                model.To = throughUtc.ToLocalTime();
            }
        }
    }
}
