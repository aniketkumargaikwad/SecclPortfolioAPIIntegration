// P1SecclApp.Core/Models/DeprecatedAuthResponse.cs
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace P1SecclApp.Core.Models
{
    public class AuthData
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("userType")]
        public string? UserType { get; set; }

        [JsonPropertyName("scopes")]
        public List<string>? Scopes { get; set; }

        [JsonPropertyName("services")]
        public List<string>? Services { get; set; }
    }

    public class DeprecatedAuthResponse
    {
        [JsonPropertyName("data")]
        public AuthData? Data { get; set; }
    }
}