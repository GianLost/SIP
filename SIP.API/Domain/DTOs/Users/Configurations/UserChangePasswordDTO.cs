namespace SIP.API.Domain.DTOs.Users.Configurations;

public class UserChangePasswordDTO
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}