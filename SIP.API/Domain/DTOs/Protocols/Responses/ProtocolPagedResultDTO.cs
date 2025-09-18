namespace SIP.API.Domain.DTOs.Protocols.Responses;

public class ProtocolPagedResultDTO
{
    public ICollection<ProtocolListItemDto>? Items { get; set; } = [];
    public int TotalCount { get; set; }
}