using SIP.API.Domain.DTOs.Protocols.Default;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users.Pagination;

public class UserListItemDTO : BaseUser
{
    public Guid Id { get; set; }

    public bool Status { get; set; }

    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string SectorAcronym { get; set; } = string.Empty;

    public ICollection<ProtocolDefaultDTO> ProtocolsCreated { get; set; } = [];
    public ICollection<ProtocolDefaultDTO> ProtocolsReceived { get; set; } = [];
}