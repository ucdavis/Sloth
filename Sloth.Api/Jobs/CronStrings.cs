namespace Sloth.Api.Jobs
{
    internal class CronStrings
    {
        /// <summary>
        /// Every Minute
        /// </summary>
        public const string Minutely = "* * * * *";

        /// <summary>
        /// Each Night
        /// </summary>
        public const string Nightly = "0 23 * * *";

        /// <summary>
        /// 4pm weekdays
        /// </summary>
        public const string EndOfBusiness = "0 14 * * 1,2,3,4,5";
    }
}
