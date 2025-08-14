using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols;

public class ProtocolCreateDTO
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OriginSectorId { get; set; }
    public Guid CreatedById { get; set; }
    public Guid DestinationSectorId { get; set; }
    public Guid DestinationUserId { get; set; }
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; }
}