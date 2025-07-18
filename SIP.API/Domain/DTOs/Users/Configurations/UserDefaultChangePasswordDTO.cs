namespace SIP.API.Domain.DTOs.Users.Configurations;

public class UserDefaultChangePasswordDTO
{
    public Guid UserId { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class ChangedPasswordResponseDTO(string message)
{
    public string Message { get; set; } = message;
}