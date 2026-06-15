using SIP.UI.Domain.DTOs.Users.Request;

namespace SIP.UI.Domain.DTOs.Users.Auth;

public class AuthResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UserRequestDTO User { get; set; } = new();
    public string Role { get; set; } = string.Empty;
}