namespace Sloth.Core.Configuration
{
    public class WebHookServiceOptions
    {
        public int RetryDelaySeconds { get; set; } = 1;

        public int MaxRetryDelaySeconds { get; set; } = 32;

        public int MinFetchAgeMinutes { get; set; } = 10;
    }
}
