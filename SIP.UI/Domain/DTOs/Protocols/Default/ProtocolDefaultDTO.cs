using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols.Default;

public class ProtocolDefaultDTO
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; }
}