using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.DTOs.Protocols.Responses;
using SIP.API.Domain.Entities.Protocols;

namespace SIP.API.Domain.Interfaces.Protocols;

public interface IProtocol
{
    Task<Protocol> CreateAsync(ProtocolCreateDTO dto);
    Task<Protocol?> GetByIdAsync(Guid id);
    Task<ICollection<Protocol>> GetAllAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);
    Task<ProtocolPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);
    Task<Protocol?> UpdateAsync(Guid id, ProtocolUpdateDTO protocol);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetTotalProtocolsCountAsync(string? searchString);
    public void ClearTotalProtocolsCountCache();
}