using System.Text.Json.Serialization;

namespace TestReserveX.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }

        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }
    }
}
