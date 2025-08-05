using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tours.Configuration;
using ToursApi.ServiceLayer.Models;

namespace Tours.Services
{
    public interface IToursProviderService

    {
        Task<IEnumerable<AutoCompleteDto>> AutoCompleteAsync(string term);
    }

    public class ToursProviderService : IToursProviderService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ToursApiSettings _settings;

        public ToursProviderService(HttpClient httpClient, ITokenService tokenService, IOptions<ToursApiSettings> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<IEnumerable<AutoCompleteDto>> AutoCompleteAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentException("Search term cannot be null or empty.", nameof(term));

            var requestUrl = $"{_settings.BaseUrl}/Search/AutoCompleteDropDown?accessToken={_settings.AccessToken}&paramSearch={term}";

            var token = await _tokenService.GetTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                token = await _tokenService.GetTokenAsync(forceRefresh: true);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                response = await _httpClient.SendAsync(request);
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reservex API failed: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<AutoCompleteDto>>()
                   ?? Enumerable.Empty<AutoCompleteDto>();
        }
    }

}