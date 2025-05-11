// P1SecclApp.Core/Services/PortfolioAggregationService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P1SecclApp.Core.Models;

namespace P1SecclApp.Core.Services
{
    public interface IPortfolioAggregationService
    {
        Task<AggregatedPortfolioTotal?> GetAggregatedTotalValueAsync(List<string> clientPortfolioIds);
        Task<AggregatedPortfolioByAccountType?> GetAggregatedTotalsByAccountTypeAsync(List<string> clientPortfolioIds);
    }

    public class PortfolioAggregationService : IPortfolioAggregationService
    {
        private readonly ISecclApiService _secclApiService;

        // For this exercise, we'll hardcode some example Client Portfolio IDs.
        // In a real application, these might come from user input, a database, or another API call.
        // You will need to find valid clientPortfolioIds by exploring the SECCL API,
        // perhaps using the "List all client portfolios" endpoint: GET /client-portfolios
        // For now, we'll use placeholders. The API will likely return 404 for these.
        // Replace these with ACTUAL IDs you discover from SECCL's staging environment.
        public static List<string> DefaultPortfolioIds = new List<string> {
            "DEMO_PORTFOLIO_ID_1", // Replace with a real ID
            "DEMO_PORTFOLIO_ID_2", // Replace with a real ID
            "DEMO_PORTFOLIO_ID_3"  // Replace with a real ID
        };


        public PortfolioAggregationService(ISecclApiService secclApiService)
        {
            _secclApiService = secclApiService;
        }

        public async Task<AggregatedPortfolioTotal?> GetAggregatedTotalValueAsync(List<string> clientPortfolioIds)
        {
            if (clientPortfolioIds == null || !clientPortfolioIds.Any())
            {
                Console.WriteLine("No portfolio IDs provided for aggregation.");
                return null; // Or throw an ArgumentNullException
            }

            var token = await _secclApiService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Failed to get access token. Cannot aggregate total value.");
                return null;
            }

            decimal totalValue = 0;
            var validPortfolioIds = new List<string>();

            foreach (var id in clientPortfolioIds)
            {
                var valuation = await _secclApiService.GetPortfolioValuationAsync(id, token);
                if (valuation != null)
                {
                    totalValue += valuation.TotalMarketValue;
                    validPortfolioIds.Add(id);
                    Console.WriteLine($"Fetched valuation for {id}: {valuation.TotalMarketValue}");
                }
                else
                {
                    Console.WriteLine($"Could not fetch valuation for portfolio ID: {id}");
                }
            }

            if (!validPortfolioIds.Any()) return null; // No data was fetched

            return new AggregatedPortfolioTotal
            {
                TotalValue = totalValue,
                PortfolioIdsIncluded = validPortfolioIds
            };
        }

        public async Task<AggregatedPortfolioByAccountType?> GetAggregatedTotalsByAccountTypeAsync(List<string> clientPortfolioIds)
        {
            if (clientPortfolioIds == null || !clientPortfolioIds.Any())
            {
                Console.WriteLine("No portfolio IDs provided for aggregation by account type.");
                return null;
            }

            var token = await _secclApiService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Failed to get access token. Cannot aggregate by account type.");
                return null;
            }

            var valuations = new List<PortfolioValuationResponse>();
            var validPortfolioIds = new List<string>();

            foreach (var id in clientPortfolioIds)
            {
                var valuation = await _secclApiService.GetPortfolioValuationAsync(id, token);
                if (valuation != null)
                {
                    valuations.Add(valuation);
                    validPortfolioIds.Add(id);
                    Console.WriteLine($"Fetched valuation for {id} (Account Type: {valuation.Account?.Type ?? "N/A"})");
                }
                else
                {
                    Console.WriteLine($"Could not fetch valuation for portfolio ID: {id} for account type aggregation.");
                }
            }

            if (!valuations.Any()) return null;

            var aggregation = valuations
                .Where(v => v.Account != null && !string.IsNullOrEmpty(v.Account.Type)) // Ensure account type exists
                .GroupBy(v => v.Account!.Type) // Group by account type
                .Select(g => new AccountTypeAggregation
                {
                    AccountType = g.Key,
                    TotalValue = g.Sum(v => v.TotalMarketValue),
                    PortfolioCount = g.Count()
                })
                .ToList();

            return new AggregatedPortfolioByAccountType
            {
                Aggregations = aggregation,
                PortfolioIdsIncluded = validPortfolioIds
            };
        }
    }
}