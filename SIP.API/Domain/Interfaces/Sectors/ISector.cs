using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.Entities.Sectors;

namespace SIP.API.Domain.Interfaces.Sectors;

/// <summary>
/// Provides an abstraction for CRUD operations on sector entities.
/// </summary>
public interface ISector
{
    /// <summary>
    /// Asynchronously creates a new sector in the database.
    /// </summary>
    /// <param name="dto">A data transfer object containing the sector's information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the created <see cref="Sector"/> entity.
    /// </returns>
    Task<Sector> CreateAsync(SectorCreateDTO dto);

    /// <summary>
    /// Asynchronously retrieves a sector by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sector.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="Sector"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Sector?> GetByIdAsync(Guid id);

    /// <summary>
    /// Asynchronously retrieves a paginated list of sectors from the database.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of <see cref="Sector"/> entities.
    /// </returns>
    Task<IEnumerable<Sector>> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Asynchronously updates an existing sector in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the sector to update.</param>
    /// <param name="dto">A data transfer object containing the updated sector values.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the updated <see cref="Sector"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Sector?> UpdateAsync(Guid id, SectorUpdateDTO dto);

    /// <summary>
    /// Asynchronously deletes a sector by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sector to delete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if the sector was deleted; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> DeleteAsync(Guid id);
}