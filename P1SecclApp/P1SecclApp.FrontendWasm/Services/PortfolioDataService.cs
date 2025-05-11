// P1SecclApp.FrontendWasm/Services/PortfolioDataService.cs
using System.Net.Http;
using System.Net.Http.Json; // For GetFromJsonAsync
using System.Threading.Tasks;
using System.Collections.Generic; // For List
using P1SecclApp.FrontendWasm.Models; // Our frontend models

namespace P1SecclApp.FrontendWasm.Services
{
    public interface IPortfolioDataService
    {
        Task<AggregatedPortfolioTotal?> GetAggregatedTotalAsync(List<string>? portfolioIds = null);
        Task<AggregatedPortfolioByAccountType?> GetAggregatedByAccountTypeAsync(List<string>? portfolioIds = null);
    }

    public class PortfolioDataService : IPortfolioDataService
    {
        private readonly HttpClient _httpClient;

        // The base URL of your BackendApi.
        // This will be configured in Program.cs
        public PortfolioDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private string BuildQueryString(List<string>? portfolioIds)
        {
            if (portfolioIds == null || !portfolioIds.Any())
                return string.Empty;
            return "?" + string.Join("&", portfolioIds.Select(id => $"ids={Uri.EscapeDataString(id)}"));
        }

        public async Task<AggregatedPortfolioTotal?> GetAggregatedTotalAsync(List<string>? portfolioIds = null)
        {
            try
            {
                string queryString = BuildQueryString(portfolioIds);
                // The URI here should match your BackendApi endpoint
                return await _httpClient.GetFromJsonAsync<AggregatedPortfolioTotal>($"api/portfolio/aggregated-total{queryString}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching aggregated total: {ex.Message}");
                return null;
            }
        }

        public async Task<AggregatedPortfolioByAccountType?> GetAggregatedByAccountTypeAsync(List<string>? portfolioIds = null)
        {
            try
            {
                string queryString = BuildQueryString(portfolioIds);
                // The URI here should match your BackendApi endpoint
                return await _httpClient.GetFromJsonAsync<AggregatedPortfolioByAccountType>($"api/portfolio/aggregated-by-accounttype{queryString}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching aggregated by account type: {ex.Message}");
                return null;
            }
        }
    }
}