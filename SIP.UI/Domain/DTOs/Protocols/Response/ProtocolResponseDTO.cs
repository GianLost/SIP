using SIP.UI.Domain.Enums;
using SIP.UI.Models.Protocols;

namespace SIP.UI.Domain.DTOs.Protocols.Response;

public class ProtocolResponseDTO : BaseProtocol
{
    public Guid Id { get; set; }

    public override int Number { get; set; }
    public override string Subject { get; set; } = string.Empty;
    public override string Description { get; set; } = string.Empty;
    public override ProtocolStatus Status { get; set; }
    public override bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override Guid CreatedById { get; set; }
    public override Guid DestinationUserId { get; set; }
    public override Guid OriginSectorId { get; set; }
    public override Guid DestinationSectorId { get; set; }
}