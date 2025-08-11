using SIP.API.Domain.Entities.Protocols;

namespace SIP.API.Domain.DTOs.Protocols.Responses;

public class ProtocolPagedResultDTO
{
    public ICollection<Protocol> Items { get; set; } = [];
    public int TotalCount { get; set; }
}