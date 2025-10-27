using SIP.UI.Domain.Enums;
using SIP.UI.Models.Users;

namespace SIP.UI.Domain.DTOs.Users.Pagination;

public class UserListItemDTO : BaseUser
{
    public Guid Id { get; set; }

    public bool Status { get; set; }

    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;
    public override RoleEnum Role { get; set; }

    public Guid SectorId { get; set; }
    public string Sector { get; set; } = string.Empty;

    public bool ShowProtocols { get; set; }
}