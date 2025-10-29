using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Protocols.Pagination;

public class ProtocolBasicListDTO
{
    public Guid Id { get; set; }
    public ProtocolStatus Status { get; set; }
    public string Number { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;

    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
}