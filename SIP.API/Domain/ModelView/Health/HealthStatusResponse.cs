namespace SIP.API.Domain.ModelView.Health;

public class HealthStatusResponse
{
    public string Status { get; set; } = "Unknown";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public IDictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>();
    public string? Message { get; set; }
}