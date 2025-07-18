using System.Text.Json.Serialization;

namespace SIP.UI.Domain.DTOs.Users.Configurations;

public class UserDefaultChangePasswordDTO
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}