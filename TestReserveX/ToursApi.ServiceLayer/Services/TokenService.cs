using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tours.Configuration;
using ToursApi.ServiceLayer.Constants;
using ToursApi.ServiceLayer.Models;

public interface ITokenService
{
    Task<string> GetTokenAsync(bool forceRefresh = false);
}

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ToursApiSettings _settings;

    public TokenService(HttpClient httpClient, IOptions<ToursApiSettings> settings, IMemoryCache memoryCache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _memoryCache.TryGetValue(ServiceConstants.TokenCacheKey, out string cachedToken))
            return cachedToken;

        if (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password))
            throw new InvalidOperationException(ErrorMessages.CredentialsMissing);

        var credentials = new { userName = _settings.Username, password = _settings.Password };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Authentication/AuthenticateUser")
        {
            Content = JsonContent.Create(credentials, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{ErrorMessages.TokenRequestFailed}: {response.StatusCode}. {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.Token))
        {
            throw new Exception(ErrorMessages.InvalidTokenResponse);
        }

        var expiration = CalculateTokenLifetime(tokenResponse.ExpiresAt);
        _memoryCache.Set(ServiceConstants.TokenCacheKey, tokenResponse.Token, expiration);

        return tokenResponse.Token;
    }

    private TimeSpan CalculateTokenLifetime(DateTime? expiresAt)
    {
        var fallback = TimeSpan.FromMinutes(_settings.DefaultTokenExpirationMinutes);
        return expiresAt is not null && expiresAt > DateTime.UtcNow
            ? expiresAt.Value - DateTime.UtcNow
            : fallback;
    }
}