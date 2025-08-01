public class ReservexApiOptions
{
    public string BaseUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string TokenBaseUrl { get; set; }
    
    public TokenEndpointsOptions TokenEndpoints { get; set; }
}

public class TokenEndpointsOptions
{
    public string Create { get; set; }
    public string Validate { get; set; }
    public string Renew { get; set; }
}
