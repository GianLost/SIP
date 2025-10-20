using SIP.UI.Models.Users;

namespace SIP.UI.Domain.DTOs.Users.Request;

public class UserDefaultRequestDTO : BaseUser
{
    public Guid Id { get; set; }

    public bool Status { get; set; }

    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    public Guid SectorId { get; set; }
}