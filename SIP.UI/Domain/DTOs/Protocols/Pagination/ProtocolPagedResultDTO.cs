namespace SIP.UI.Domain.DTOs.Protocols.Pagination;

public class ProtocolPagedResultDTO
{
    public ICollection<ProtocolListItemDto>? Items { get; set; } = [];
    public int TotalCount { get; set; }
}