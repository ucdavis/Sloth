namespace Sloth.Core.Configuration
{
    public class AggieEnterpriseOptions
    {
        public string GraphQlUrl { get; set; }
        public string Token { get; set; }

        public string JournalSource { get; set; }

        public string JournalCategory { get; set; }

        public bool BatchRequest { get; set; } = true;
    }
}
