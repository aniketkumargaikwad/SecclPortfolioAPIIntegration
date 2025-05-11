// P1SecclApp.FrontendWasm/Models/AggregatedPortfolioInfo.cs
// These should match the DTOs returned by your BackendApi
namespace P1SecclApp.FrontendWasm.Models
{
    public class AggregatedPortfolioTotal
    {
        public decimal TotalValue { get; set; }
        public List<string>? PortfolioIdsIncluded { get; set; }
    }

    public class AccountTypeAggregation
    {
        public string? AccountType { get; set; }
        public decimal TotalValue { get; set; }
        public int PortfolioCount { get; set; }
    }

    public class AggregatedPortfolioByAccountType
    {
        public List<AccountTypeAggregation>? Aggregations { get; set; }
        public List<string>? PortfolioIdsIncluded { get; set; }
    }
}