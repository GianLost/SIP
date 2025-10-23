using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Protocols;

namespace SIP.API.Domain.DTOs.Protocols.Default;

public class ProtocolDefaultDTO : BaseProtocol
{
    public Guid Id { get; set; }
    public override int Number { get; set; }
    public override string Subject { get; set; } = string.Empty;
    public override string Description { get; set; } = string.Empty;
    public override ProtocolStatus Status { get; set; }
    public override bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; }
}