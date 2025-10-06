namespace SIP.UI.Models.Errors;

/// <summary>
/// Model for error responses from the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// The error message returned by the API.
    /// </summary>
    public string? Error { get; set; }
}