using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.DTOs.Protocols.Pagination;
using SIP.API.Domain.DTOs.Protocols.Responses;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Interfaces.Default;

namespace SIP.API.Domain.Interfaces.Protocols;

public interface IProtocol : IEntityManager<Protocol, ProtocolCreateDTO, ProtocolUpdateDTO, ProtocolBasicListDTO, ProtocolResponseDTO>
{
    string FormatProtocolNumber(int nextSequence);
    int GetNextSequence(string? lastProtocolNumber);
    Task<string?> GetLastProtocolNumberAsync();
    Task<string> GenerateProtocolNumberAsync();
}