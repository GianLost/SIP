using SIP.UI.Domain.Enums;
using SIP.UI.Domain.DTOs.Sectors.Responses;
using SIP.UI.Domain.DTOs.Users.Responses;

namespace SIP.UI.Domain.DTOs.Protocols.Responses;

public class ProtocolResponseDTO
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public UserResponseDTO? CreatedBy { get; set; }
    public Guid DestinationSectorId { get; set; }
    public SectorResponseDTO? DestinationSector { get; set; }
}