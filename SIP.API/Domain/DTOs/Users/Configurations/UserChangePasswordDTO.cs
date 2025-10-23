namespace SIP.API.Domain.DTOs.Users.Configurations;

public class UserChangePasswordDTO
{
    public Guid Id { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}