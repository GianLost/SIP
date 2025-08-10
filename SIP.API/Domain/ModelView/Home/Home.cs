namespace SIP.API.Domain.ModelView.Home;

public struct Home(string documentationUrl)
{
    public readonly string Message { get; init; } = "Welcome to SIP.API v1.0";
    public string Documentation { get; set; } = documentationUrl;
}