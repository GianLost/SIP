using SIP.API.Domain.Enums;

namespace SIP.API.Domain.DTOs.Protocols.Pagination;

public class ProtocolBasicListDTO
{
    public Guid Id { get; set; }
    public ProtocolStatus Status { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}