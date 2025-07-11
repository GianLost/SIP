using System.Text.Json.Serialization;

namespace SIP.API.Controllers.Sectors;

public class ErrorResponse(string error)
{
    [JsonPropertyName("Error")]
    public string Error { get; set; } = error;
}