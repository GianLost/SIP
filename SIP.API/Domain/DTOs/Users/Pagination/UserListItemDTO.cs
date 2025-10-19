using SIP.API.Domain.DTOs.Protocols.Default;

namespace SIP.API.Domain.DTOs.Users.Pagination;

public class UserListItemDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool Status { get; set; }

    public int Masp { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;

    public string SectorAcronym { get; set; } = string.Empty;

    public ICollection<ProtocolDefaultDTO> ProtocolsCreated { get; set; } = [];
    public ICollection<ProtocolDefaultDTO> ProtocolsReceived { get; set; } = [];
}