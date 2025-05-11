using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P1SecclApp.Core.Models
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
