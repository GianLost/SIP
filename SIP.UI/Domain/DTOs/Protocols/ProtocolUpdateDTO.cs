using SIP.API.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols;

public class ProtocolUpdateDTO
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public Guid DestinationSectorId { get; set; }
}