using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols;

public class ProtocolUpdateDTO
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Subject { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public Guid DestinationSectorId { get; set; }
}