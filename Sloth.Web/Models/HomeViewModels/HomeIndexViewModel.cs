using System.Collections.Generic;

namespace Sloth.Web.Models.HomeViewModels
{
    public class HomeIndexViewModel
    {
        public IReadOnlyList<HomeTeamSummaryViewModel> Teams { get; set; } = new List<HomeTeamSummaryViewModel>();
    }

    public class HomeTeamSummaryViewModel
    {
        public string Name { get; set; }

        public string Slug { get; set; }

        public IReadOnlyList<string> SourceNames { get; set; } = new List<string>();

        public int FailedTransactionCount { get; set; }

        public int StuckTransactionCount { get; set; }
    }
}
