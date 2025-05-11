using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace P1SecclApp.Core.Models
{
    public class AccountDetail
    {
        [JsonPropertyName("accountCode")]
        public string? AccountCode { get; set; } // e.g. "ISA", "GIA"

        [JsonPropertyName("type")]
        public string? Type { get; set; } // Usually matches AccountCode or is more descriptive
    }

    public class ClientPortfolio
    {
        [JsonPropertyName("clientPortfolioId")]
        public string? ClientPortfolioId { get; set; }

        [JsonPropertyName("totalMarketValue")]
        public decimal TotalMarketValue { get; set; }

        [JsonPropertyName("account")]
        public AccountDetail? Account { get; set; }
    }

    public class PortfolioValuationResponse
    {
        // The API returns a list of these directly if successful
        // For this example, we'll assume the API structure based on the endpoint's purpose.
        // The "Get client portfolio valuation" endpoint returns a single ClientPortfolio object, not a list.
        // The structure for "Balances and valuations" in Postman shows one main object.
        // Let's assume for /portfolio-valuations/{clientPortfolioId} it's a single object
        // For simplicity, let's rename ClientPortfolio to PortfolioDetail for a single item response.

        [JsonPropertyName("clientPortfolioId")]
        public string? ClientPortfolioId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; } // e.g. "Mr John Doe - Portfolio"

        [JsonPropertyName("totalMarketValue")]
        public decimal TotalMarketValue { get; set; }

        [JsonPropertyName("account")]
        public AccountDetail? Account { get; set; }

        // Add other relevant fields if needed, like holdings, cash, etc.
        // For now, totalMarketValue and account.type are key.
    }
}
