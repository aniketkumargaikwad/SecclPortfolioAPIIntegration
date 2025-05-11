// P1SecclApp.Core/Services/SecclApiService.cs
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using P1SecclApp.Core.Models; // To access our model classes
using Microsoft.Extensions.Configuration; // For appsettings (later)

namespace P1SecclApp.Core.Services
{
    public interface ISecclApiService
    {
        Task<string?> GetAccessTokenAsync();
        Task<PortfolioValuationResponse?> GetPortfolioValuationAsync(string clientPortfolioId, string token);
    }

    public class SecclApiService : ISecclApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration; // To read credentials
        private const string SecclBaseUrl = "https://pfolio-api-staging.seccl.tech"; // As provided

        // Store credentials securely. For this exercise, we'll get them from config.
        // In a real app, use User Secrets for local dev and Azure Key Vault for production.
        private string _firmId = "";
        private string _username = "";
        private string _password = "";


        public SecclApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration; // Injected dependency

            // Load credentials from configuration
            _firmId = _configuration["SecclApi:FirmId"] ?? "P1IMX"; // Fallback for safety
            _username = _configuration["SecclApi:Username"] ?? "nelahi6642@4tmail.net";
            _password = _configuration["SecclApi:Password"] ?? "DemoBDM1";

            if (string.IsNullOrEmpty(_firmId) || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            {
                // In a real app, you might throw an exception or log a critical error
                Console.WriteLine("Error: SECCL API credentials not configured properly!");
                // For this exercise, we'll let it proceed with hardcoded fallbacks if config is missing,
                // but proper configuration is key.
            }
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var requestUrl = $"{SecclBaseUrl}/connect/token";
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("firmid", _firmId),
                new KeyValuePair<string, string>("username", _username),
                new KeyValuePair<string, string>("password", _password)
            });

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, requestBody);
                response.EnsureSuccessStatusCode(); // Throws an exception if not successful

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<SecclTokenResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // Make deserialization flexible

                return tokenResponse?.AccessToken;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error getting access token: {e.Message}");
                // You might want to log more details, like response content if available
                return null;
            }
        }

        public async Task<PortfolioValuationResponse?> GetPortfolioValuationAsync(string clientPortfolioId, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(clientPortfolioId))
            {
                Console.WriteLine("Token or ClientPortfolioId is missing for GetPortfolioValuationAsync.");
                return null;
            }

            var requestUrl = $"{SecclBaseUrl}/portfolio-valuations/{clientPortfolioId}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Portfolio with ID '{clientPortfolioId}' not found.");
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                // The SECCL API might return an array even for a single portfolio ID.
                // Let's adjust based on typical REST patterns or if the docs clarify.
                // The docs for "Get client portfolio valuation" show a single object.
                var valuation = JsonSerializer.Deserialize<PortfolioValuationResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return valuation;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error getting portfolio valuation for {clientPortfolioId}: {e.Message}");
                return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error deserializing portfolio valuation for {clientPortfolioId}: {e.Message}");
                return null;
            }
        }
    }
}