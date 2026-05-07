using SIP.API.Domain.DTOs.Users.Responses;

namespace SIP.API.Domain.DTOs.Users.Auth;

public class AuthResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UserResponseDTO User { get; set; } = new UserResponseDTO();
    public string Role { get; set; } = string.Empty;
}
