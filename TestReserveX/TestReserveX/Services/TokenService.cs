using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using TestReserveX.Models;

public class TokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    private const string TokenCacheKey = "ReservexApiToken";

    public TokenService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _memoryCache.TryGetValue(TokenCacheKey, out string cachedToken))
        {
            return cachedToken;
        }

        var userName = _configuration["ReservexApi:Username"];
        var password = _configuration["ReservexApi:Password"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Reservex API credentials are not properly configured.");
        }

        var credentials = new { userName, password };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Authentication/AuthenticateUser")
        {
            Content = JsonContent.Create(
                credentials,
                options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            )
        };

        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to fetch token. Status: {response.StatusCode}. Response: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.Token))
        {
            throw new Exception("Invalid token response received from Reservex API.");
        }

        var expiration = CalculateTokenLifetime(tokenResponse.ExpiresAt);

        _memoryCache.Set(TokenCacheKey, tokenResponse.Token, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });

        return tokenResponse.Token;
    }

    private TimeSpan CalculateTokenLifetime(DateTime? expiresAt)
    {
        var fallback = TimeSpan.FromMinutes(
            int.TryParse(_configuration["ReservexApi:DefaultTokenExpirationMinutes"], out var minutes)
                ? minutes
                : 10);

        if (expiresAt == null || expiresAt <= DateTime.UtcNow)
            return fallback;

        var ttl = expiresAt.Value - DateTime.UtcNow;
        return ttl > TimeSpan.Zero ? ttl : fallback;
    }
}
