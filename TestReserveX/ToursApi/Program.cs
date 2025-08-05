
using Microsoft.Extensions.DependencyInjection;
using Tours.Configuration;
using Tours.Constants;
using Tours.Services;
using ToursApi.Mapping;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Bind and validate configuration
// ----------------------------
var reservexApiSection = builder.Configuration.GetSection(ServiceConstants.ConfigKeys.ConfigSectionName);
builder.Services.Configure<ToursApiSettings>(reservexApiSection);

var reservexApiSettings = reservexApiSection.Get<ToursApiSettings>();
if (reservexApiSettings == null ||
    string.IsNullOrWhiteSpace(reservexApiSettings.BaseUrl) ||
    string.IsNullOrWhiteSpace(reservexApiSettings.TokenBaseUrl) ||
    string.IsNullOrWhiteSpace(reservexApiSettings.Username) ||
    string.IsNullOrWhiteSpace(reservexApiSettings.Password))
{
    throw new InvalidOperationException("ReservexApi configuration section is missing required values.");
}

// ----------------------------
// Register Core Services
// ----------------------------
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

// Register TokenService with typed HttpClient
builder.Services.AddHttpClient<ITokenService, TokenService>(client =>
{
    client.BaseAddress = new Uri(reservexApiSettings.TokenBaseUrl);
});

// ----------------------------
// Register AutoMapper and Mapping Profiles
// ----------------------------
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoCompleteMappingProfile>();
});

// Register ReservexService with typed HttpClient
builder.Services.AddHttpClient<IToursProviderService, ToursProviderService>(client =>
{
    client.BaseAddress = new Uri(reservexApiSettings.BaseUrl);
});
 



// ----------------------------
// Build and run the app
// ----------------------------
var app = builder.Build();
app.MapControllers();
app.Run();
