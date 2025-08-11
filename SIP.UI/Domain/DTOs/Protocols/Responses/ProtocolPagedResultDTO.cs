using SIP.UI.Models.Protocols;

namespace SIP.UI.Domain.DTOs.Protocols.Responses;

public class ProtocolPagedResultDTO
{
    public ICollection<Protocol>? Items { get; set; }
    public int TotalCount { get; set; }
}