using System.Text.Json.Serialization;

namespace SIP.API.Controllers.Errors;

public class ErrorResponse(string error)
{
    [JsonPropertyName("Error")]
    public string Error { get; set; } = error;
}