using SIP.API.Domain.Enums;

namespace SIP.API.Domain.DTOs.Protocols.Responses;

public class ProtocolListItemDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string CreatedByFullName { get; set; } = string.Empty;
    public string DestinationUserFullName { get; set; } = string.Empty;
    public string OriginSectorAcronym { get; set; } = string.Empty;
    public string DestinationSectorAcronym { get; set; } = string.Empty;
}