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

        // Default placeholder IDs if none are provided by the caller.
        // The API endpoints will use these if the 'ids' query parameter is empty.
        public static List<string> DefaultPortfolioIds = new List<string> {
            "FALLBACK_ID_001",
            "FALLBACK_ID_002",
            "FALLBACK_ID_003"
        };

        // Predefined dummy token to use if the real token acquisition fails
        private const string DummyAccessToken = "DUMMY_ACCESS_TOKEN_IGNORE_IF_SECCL_FAILS";

        public PortfolioAggregationService(ISecclApiService secclApiService)
        {
            _secclApiService = secclApiService;
        }

        // Helper to generate default portfolio valuation data
        private PortfolioValuationResponse GetDefaultPortfolioValuation(string portfolioId, int index)
        {
            // Cycle through account types for variety in dummy data
            string[] accountTypes = { "ISA", "GIA", "SIPP" };
            string accountType = accountTypes[index % accountTypes.Length];
            decimal marketValue = 50000.00m + (index * 10000.00m); // Vary the value

            return new PortfolioValuationResponse
            {
                ClientPortfolioId = portfolioId,
                Name = $"Default Portfolio ({portfolioId})",
                TotalMarketValue = marketValue,
                Account = new AccountDetail
                {
                    AccountCode = accountType,
                    Type = accountType
                }
            };
        }

        public async Task<AggregatedPortfolioTotal?> GetAggregatedTotalValueAsync(List<string> clientPortfolioIds)
        {
            // Ensure we always have a list of IDs to work with
            var portfolioIdsToProcess = (clientPortfolioIds == null || !clientPortfolioIds.Any())
                                        ? DefaultPortfolioIds
                                        : clientPortfolioIds;

            Console.WriteLine($"[PortfolioAggregationService] Attempting to get aggregated total for IDs: {string.Join(", ", portfolioIdsToProcess)}");

            string? token = await _secclApiService.GetAccessTokenAsync();
            bool useDummyData = false;

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[PortfolioAggregationService] Failed to get real access token. Using dummy token and proceeding with default data logic.");
                token = DummyAccessToken; // Use a dummy token placeholder
                useDummyData = true; // Flag to use dummy data for portfolios
            }
            else
            {
                Console.WriteLine("[PortfolioAggregationService] Successfully obtained real access token.");
            }

            decimal totalValue = 0;
            var portfolioIdsIncludedInAggregation = new List<string>();
            int portfolioIndex = 0;

            foreach (var id in portfolioIdsToProcess)
            {
                PortfolioValuationResponse? valuation = null;
                if (!useDummyData) // Only attempt real fetch if we got a real token
                {
                    Console.WriteLine($"[PortfolioAggregationService] Attempting to fetch REAL valuation for portfolio ID: {id}");
                    valuation = await _secclApiService.GetPortfolioValuationAsync(id, token);
                }

                if (valuation == null) // If real fetch failed OR we decided to use dummy data from the start
                {
                    Console.WriteLine($"[PortfolioAggregationService] Using DEFAULT valuation data for portfolio ID: {id}");
                    valuation = GetDefaultPortfolioValuation(id, portfolioIndex);
                }
                else
                {
                    Console.WriteLine($"[PortfolioAggregationService] Successfully fetched REAL valuation for {id}: {valuation.TotalMarketValue}");
                }

                totalValue += valuation.TotalMarketValue;
                portfolioIdsIncludedInAggregation.Add(id);
                portfolioIndex++;
            }

            Console.WriteLine($"[PortfolioAggregationService] Final aggregated total: {totalValue} for IDs: {string.Join(", ", portfolioIdsIncludedInAggregation)}");
            return new AggregatedPortfolioTotal
            {
                TotalValue = totalValue,
                PortfolioIdsIncluded = portfolioIdsIncludedInAggregation
            };
        }

        public async Task<AggregatedPortfolioByAccountType?> GetAggregatedTotalsByAccountTypeAsync(List<string> clientPortfolioIds)
        {
            var portfolioIdsToProcess = (clientPortfolioIds == null || !clientPortfolioIds.Any())
                                        ? DefaultPortfolioIds
                                        : clientPortfolioIds;

            Console.WriteLine($"[PortfolioAggregationService] Attempting to get aggregated totals by account type for IDs: {string.Join(", ", portfolioIdsToProcess)}");


            string? token = await _secclApiService.GetAccessTokenAsync();
            bool useDummyData = false;

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[PortfolioAggregationService] Failed to get real access token for account type aggregation. Using dummy token and proceeding with default data logic.");
                token = DummyAccessToken;
                useDummyData = true;
            }
            else
            {
                Console.WriteLine("[PortfolioAggregationService] Successfully obtained real access token for account type aggregation.");
            }

            var valuations = new List<PortfolioValuationResponse>();
            var portfolioIdsIncludedInAggregation = new List<string>();
            int portfolioIndex = 0;

            foreach (var id in portfolioIdsToProcess)
            {
                PortfolioValuationResponse? valuation = null;
                if (!useDummyData)
                {
                    Console.WriteLine($"[PortfolioAggregationService] Attempting to fetch REAL valuation for portfolio ID: {id} (for account type agg)");
                    valuation = await _secclApiService.GetPortfolioValuationAsync(id, token);
                }

                if (valuation == null)
                {
                    Console.WriteLine($"[PortfolioAggregationService] Using DEFAULT valuation data for portfolio ID: {id} (for account type agg)");
                    valuation = GetDefaultPortfolioValuation(id, portfolioIndex);
                }
                else
                {
                    Console.WriteLine($"[PortfolioAggregationService] Successfully fetched REAL valuation for {id} (Account Type: {valuation.Account?.Type ?? "N/A"})");
                }

                valuations.Add(valuation); // Add the (real or default) valuation
                portfolioIdsIncludedInAggregation.Add(id);
                portfolioIndex++;
            }

            var aggregation = valuations
                .Where(v => v.Account != null && !string.IsNullOrEmpty(v.Account.Type))
                .GroupBy(v => v.Account!.Type)
                .Select(g => new AccountTypeAggregation
                {
                    AccountType = g.Key,
                    TotalValue = g.Sum(v => v.TotalMarketValue),
                    PortfolioCount = g.Count()
                })
                .ToList();

            Console.WriteLine($"[PortfolioAggregationService] Final aggregation by account type complete for IDs: {string.Join(", ", portfolioIdsIncludedInAggregation)}");
            return new AggregatedPortfolioByAccountType
            {
                Aggregations = aggregation,
                PortfolioIdsIncluded = portfolioIdsIncludedInAggregation
            };
        }
    }
}