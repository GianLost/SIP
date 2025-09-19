using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Users;

public class UserCreateDTO
{
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Masp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    public RoleEnum Role { get; set; }

    public Guid SectorId { get; set; }
}