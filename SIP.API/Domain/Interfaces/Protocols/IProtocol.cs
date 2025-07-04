using SIP.API.Domain.Entities.Protocols;

namespace SIP.API.Domain.Interfaces.Protocols;

public interface IProtocol
{
    Task<Protocol> CreateAsync(Protocol protocol);
    Task<Protocol?> GetByIdAsync(Guid id);
    Task<IEnumerable<Protocol>> GetAllAsync();
    Task<Protocol?> UpdateAsync(Protocol protocol);
    Task<bool> DeleteAsync(Guid id);
}