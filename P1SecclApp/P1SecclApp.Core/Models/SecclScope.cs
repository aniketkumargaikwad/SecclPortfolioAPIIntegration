using System.Text.Json.Serialization;

namespace P1SecclApp.Core.Models
{
    public class SecclScope
    {
        [JsonPropertyName("scope")]
        public string? ScopeName { get; set; } // Renamed to avoid conflict if nested, or just 'Scope'

        [JsonPropertyName("ranges")]
        public List<string>? Ranges { get; set; } // This can be null if not present
    }
}
