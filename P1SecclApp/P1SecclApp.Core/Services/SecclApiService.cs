// P1SecclApp.Core/Services/SecclApiService.cs
using System;
using System.Net.Http;
using System.Net.Http.Headers; // Keep for GetPortfolioValuationAsync
using System.Net.Http.Json;    // For PostAsJsonAsync or creating JsonContent
using System.Text;             // For StringContent
using System.Text.Json;
using System.Threading.Tasks;
using P1SecclApp.Core.Models;
using Microsoft.Extensions.Configuration;

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
        private readonly IConfiguration _configuration;
        private const string SecclBaseApiUrl = "https://pfolio-api-staging.seccl.tech"; // {{apiRoute}}

        private readonly string _firmId;
        private readonly string _username; // This is the 'id' field for SECCL
        private readonly string _password;

        public SecclApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _firmId = _configuration["SecclApi:FirmId"] ?? throw new InvalidOperationException("SecclApi:FirmId is not configured.");
            _username = _configuration["SecclApi:Username"] ?? throw new InvalidOperationException("SecclApi:Username is not configured.");
            _password = _configuration["SecclApi:Password"] ?? throw new InvalidOperationException("SecclApi:Password is not configured.");
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            // Correct endpoint for the deprecated username/password flow
            var requestUrl = $"{SecclBaseApiUrl}/authenticate";

            // Create the JSON request body object
            var requestPayload = new
            {
                firmId = _firmId,
                id = _username, // SECCL documentation uses 'id' for the username/email
                password = _password
            };

            // Serialize the payload to JSON and create StringContent
            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var requestBody = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            Console.WriteLine($"[SecclApiService] Attempting to get access token from: {requestUrl}");
            Console.WriteLine($"[SecclApiService] Request Payload: {jsonPayload}");

            try
            {
                // Make the POST request with the JSON body
                var response = await _httpClient.PostAsync(requestUrl, requestBody);

                var responseContentString = await response.Content.ReadAsStringAsync(); // Read content for logging
                Console.WriteLine($"[SecclApiService] Token Response Status Code: {response.StatusCode}");
                Console.WriteLine($"[SecclApiService] Token Response Content: {responseContentString}");

                response.EnsureSuccessStatusCode(); // Throws an exception if not successful (e.g., 400, 401, 500)

                var authResponse = JsonSerializer.Deserialize<DeprecatedAuthResponse>(responseContentString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (authResponse?.Data?.Token != null)
                {
                    Console.WriteLine($"[SecclApiService] Successfully retrieved token.");
                    return authResponse.Data.Token;
                }
                else
                {
                    Console.WriteLine("[SecclApiService] Token not found in response data or response format unexpected.");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"---------- ERROR GETTING ACCESS TOKEN (HttpRequestException) ----------");
                Console.WriteLine($"Message: {e.Message}");
                if (e.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception Message: {e.InnerException.Message}");
                }
                if (e.StatusCode.HasValue)
                {
                    Console.WriteLine($"Status Code From Exception: {e.StatusCode.Value}");
                }
                Console.WriteLine($"Request URL: {requestUrl}");
                Console.WriteLine($"------------------------------------------------");
                return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"---------- ERROR DESERIALIZING TOKEN RESPONSE (JsonException) ----------");
                Console.WriteLine($"Message: {e.Message}");
                //Console.WriteLine($"Response content that failed to deserialize: {await e.PathAsync() /* This is a placeholder, actual content would be from responseContentString */ }");
                Console.WriteLine($"------------------------------------------------");
                return null;
            }
            catch (Exception e) // Catch any other unexpected errors
            {
                Console.WriteLine($"---------- UNEXPECTED ERROR GETTING ACCESS TOKEN ({e.GetType().Name}) ----------");
                Console.WriteLine($"Message: {e.Message}");
                if (e.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception Message: {e.InnerException.Message}");
                }
                Console.WriteLine($"------------------------------------------------");
                return null;
            }
        }

        public async Task<PortfolioValuationResponse?> GetPortfolioValuationAsync(string clientPortfolioId, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(clientPortfolioId))
            {
                Console.WriteLine("[SecclApiService] Token or ClientPortfolioId is missing for GetPortfolioValuationAsync.");
                return null;
            }

            // The deprecated flow documentation for /authenticate says the token should be used in 'api-token' header.
            // This is different from the more common 'Bearer' token in an 'Authorization' header.
            // Let's verify if the subsequent /portfolio-valuations endpoint also expects 'api-token' or 'Authorization: Bearer'.
            // The original problem statement did not specify the header for data requests, only for token generation.
            // Assuming subsequent calls might still use Bearer token for simplicity, unless SECCL is very different.
            // If portfolio calls fail with this, we might need to change this to use 'api-token' header.

            var requestUrl = $"{SecclBaseApiUrl}/portfolio-valuations/{clientPortfolioId}";
            _httpClient.DefaultRequestHeaders.Authorization = null; // Clear previous auth if any
            _httpClient.DefaultRequestHeaders.Remove("api-token");   // Clear previous custom token if any

            // The SECCL /authenticate documentation mentions: "API token ... should be used in all subsequent calls ... in the api-token header field."
            // So, we should use 'api-token' header instead of 'Authorization: Bearer token'.
            _httpClient.DefaultRequestHeaders.Add("api-token", token);
            // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // This is likely WRONG for SECCL if following deprecated flow strictly

            Console.WriteLine($"[SecclApiService] Attempting to get valuation for {clientPortfolioId} using token in 'api-token' header.");

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                var responseContentString = await response.Content.ReadAsStringAsync(); // Read content for logging

                Console.WriteLine($"[SecclApiService] Valuation Response Status Code for {clientPortfolioId}: {response.StatusCode}");
                // Avoid logging potentially large successful valuation responses unless debugging
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SecclApiService] Valuation Response Content for {clientPortfolioId}: {responseContentString}");
                }


                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"[SecclApiService] Portfolio with ID '{clientPortfolioId}' not found (404 from SECCL).");
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var valuation = JsonSerializer.Deserialize<PortfolioValuationResponse>(responseContentString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Console.WriteLine($"[SecclApiService] Successfully fetched valuation for {clientPortfolioId}.");
                return valuation;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"[SecclApiService] Error getting portfolio valuation for {clientPortfolioId}: {e.Message}");
                if (e.StatusCode.HasValue) Console.WriteLine($"[SecclApiService] Status Code From Exception: {e.StatusCode.Value}");
                return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"[SecclApiService] Error deserializing portfolio valuation for {clientPortfolioId}: {e.Message}");
                return null;
            }
            finally
            {
                // Clean up custom header for next request (if HttpClient is reused broadly)
                _httpClient.DefaultRequestHeaders.Remove("api-token");
            }
        }
    }
}