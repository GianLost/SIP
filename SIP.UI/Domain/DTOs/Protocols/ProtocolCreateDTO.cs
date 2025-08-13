using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols;

public class ProtocolCreateDTO
{
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public Guid CreatedById { get; set; }
    public Guid DestinationSectorId { get; set; }
}