namespace Tours.Configuration
{
      public class ToursApiSettings
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string TokenBaseUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int DefaultTokenExpirationMinutes { get; set; } = 10;
    }
}
