using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols.Responses;

public class ProtocolListItemDto
{
    public Guid Id { get; set; }
    public string? Number { get; set; } = string.Empty;
    public string? Subject { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? CreatedByFullName { get; set; }
    public string? DestinationUserFullName { get; set; }
    public string? OriginSectorAcronym { get; set; }
    public string? DestinationSectorAcronym { get; set; }
}