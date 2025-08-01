using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using TestReserveX.Models;

public class ReservexService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public ReservexService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<SearchResultDto>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            throw new ArgumentException("Search term cannot be null or empty.", nameof(term));

        var baseUrl = _configuration["ReservexApi:BaseUrl"]
                      ?? throw new InvalidOperationException("ReservexApi BaseUrl is not configured.");

        var accessToken = _configuration["ReservexApi:AccessToken"]
                          ?? throw new InvalidOperationException("ReservexApi AccessToken is not configured.");

        var requestUrl = $"{baseUrl}/api/Search/AutoCompleteDropDown?accessToken={accessToken}&paramSearch={term}";

        var token = await _tokenService.GetTokenAsync();
        SetAuthorizationHeader(token);

        var response = await _httpClient.GetAsync(requestUrl);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Retry with refreshed token
            token = await _tokenService.GetTokenAsync(forceRefresh: true);
            SetAuthorizationHeader(token);

            response = await _httpClient.GetAsync(requestUrl);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Reservex API request failed with status {response.StatusCode}: {error}");
        }

        var searchResults = await response.Content.ReadFromJsonAsync<IEnumerable<SearchResultDto>>();
        return searchResults ?? Enumerable.Empty<SearchResultDto>();
    }

    private void SetAuthorizationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
