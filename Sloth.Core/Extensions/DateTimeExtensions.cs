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
        /// Returns the Fiscal Year according to the date provided to be used for transactions.
        /// </summary>
        /// <returns>Fiscal Year</returns>
        public static int FiscalYear(this DateTime datetime)
        {
            if (datetime == null)
            {
                throw new ArgumentNullException(nameof(datetime));
            }

            // Fiscal dates for KFS are PST
            datetime = datetime.ToPacificTime();

            var year = datetime.Year;
            var month = datetime.Month;

            // Use the current year if prior to July 1st;
            // Otherwise use the next calendar year.
            if (month < 7)
                return year;

            return (year + 1);
        }

        /// <summary>
        /// Returns the Fiscal Period according to date provided to be used for transactions.
        /// </summary>
        /// <returns>Fiscal Period string</returns>
        public static int FiscalPeriod(this DateTime datetime)
        {
            if (datetime == null)
            {
                throw new ArgumentNullException(nameof(datetime));
            }

            // Fiscal dates for KFS are PST
            datetime = datetime.ToPacificTime();

            var month = datetime.Month;

            // return the calculated period:
            // Subtract 6 from the current month if 
            // after June 30th and prior to January 1st;
            // Otherwise add 6 to the current month.
            if (month > 6 && month <= 12)
                return (month - 6);

            return (month + 6);
        }
    }
}
