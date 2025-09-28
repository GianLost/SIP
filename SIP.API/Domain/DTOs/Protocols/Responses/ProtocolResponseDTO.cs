using SIP.API.Domain.Enums;
using SIP.API.Domain.Helpers.StatusHelper;
using SIP.API.Domain.Models.Protocols;

namespace SIP.API.Domain.DTOs.Protocols.Responses;

public class ProtocolResponseDTO : BaseProtocol
{
    public Guid Id { get; set; }

    public override int Number { get; set; }
    public override string Subject { get; set; } = string.Empty;
    public override string Description { get; set; } = string.Empty;
    public override ProtocolStatus Status { get; set; }
    public override bool IsArchived { get; set; } = false;

    public string CreatedByName { get; set; } = string.Empty;
    public string DestinationUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string OriginSectorAcronym { get; set; } = string.Empty;
    public string DestinationSectorAcronym { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; } = null;
    public string? UpdatedByName { get; set; } = string.Empty;

    public string StatusName => Status.ToFriendlyName();
}