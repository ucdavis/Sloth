using System;

namespace Sloth.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static readonly TimeZoneInfo Pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        public static DateTime ToPacificTime(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, Pacific);
        }

        public static DateTime? ToPacificTime(this DateTime? dateTime)
        {
            return dateTime?.ToPacificTime();
        }

        public static DateTime FromPacificTime(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, Pacific);
        }

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
