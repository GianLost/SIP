using SIP.API.Domain.Entities.Movements;

namespace SIP.API.Domain.Interfaces.Movements;

public interface IMovement
{
    Task<Movement> CreateAsync(Movement movement);
    Task<Movement?> GetByIdAsync(Guid id);
    Task<IEnumerable<Movement>> GetAllAsync();
    Task<Movement?> UpdateAsync(Movement movement);
    Task<bool> DeleteAsync(Guid id);
}