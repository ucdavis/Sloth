namespace Sloth.Core.Configuration
{
    public class AggieEnterpriseOptions
    {
        public string GraphQlUrl { get; set; }
        public string Token { get; set; }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string TokenEndpoint { get; set; }
        public string ScopeApp { get; set; }
        public string ScopeEnv { get; set; }

        public string JournalSource { get; set; }

        public string JournalCategory { get; set; }

        public bool BatchRequest { get; set; } = true;

        public bool DisableJournalUpload { get; set; } = false;
    }
}
