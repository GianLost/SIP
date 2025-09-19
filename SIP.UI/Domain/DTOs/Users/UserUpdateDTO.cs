using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Users;

public class UserUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Masp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public RoleEnum Role { get; set; }
}