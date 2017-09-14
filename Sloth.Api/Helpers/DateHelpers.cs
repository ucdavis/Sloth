using System;

namespace Sloth.Api.Helpers
{
    public static class DateHelpers
    {
        /// <summary>
        /// Fiscal Year runs from July 1 to Jun 30
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetFinancialYear(this DateTime date)
        {
            return date.Month >= 7 ? date.Year + 1 : date.Year;
        }

        /// <summary>
        ///  Fiscal Period is the month designator for the fiscal year.
        ///    So Jul = 1, Dec = 6, Jan = 7, Jun = 12
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetFiscalPeriod(this DateTime date)
        {
            return (date.Month + 5) % 12 + 1;
        }
    }
}
