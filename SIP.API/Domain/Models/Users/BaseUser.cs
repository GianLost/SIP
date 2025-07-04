using System.Text.Json.Serialization;

namespace SIP.API.Domain.Models.Users;

/// <summary>
/// Provides base properties for user data transfer objects (DTOs).
/// </summary>
public class BaseUser
{
    [JsonPropertyOrder(1)]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyOrder(3)]
    public int Masp { get; set; }

    [JsonPropertyOrder(4)]
    public string Email { get; set; } = string.Empty;
}