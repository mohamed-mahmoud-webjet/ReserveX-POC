var builder = WebApplication.CreateBuilder(args);

// Validate required configuration values  
ValidateConfigKey(builder.Configuration, "ReservexApi:TokenBaseUrl");
ValidateConfigKey(builder.Configuration, "ReservexApi:BaseUrl");

// Register TokenService with a typed HttpClient
builder.Services.AddHttpClient<TokenService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ReservexApi:TokenBaseUrl"]!);
});

// Register ReservexService with a typed HttpClient
builder.Services.AddHttpClient<ReservexService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ReservexApi:BaseUrl"]!);
});

// Core services
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

 

var app = builder.Build();
app.MapControllers();
app.Run();

 
static void ValidateConfigKey(IConfiguration config, string key)
{
    if (string.IsNullOrWhiteSpace(config[key]))
    {
        throw new InvalidOperationException($"Configuration key '{key}' is missing or empty.");
    }
}