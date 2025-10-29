using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols.Pagination;

public class ProtocolListItemDto
{
    public Guid Id { get; set; }
    public ProtocolStatus Status { get; set; }
    public int Number { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid CreatedById { get; set; }
    public bool IsArchived { get; set; } = false;
}