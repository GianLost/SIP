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
    /// Retrieves all sectors records.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list with all <see cref="Sector"/> entities
    /// </returns>
    Task<List<Sector>> GetAllSectorsAsync();

    /// <summary>
    /// Gets a paginated result of sectors from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A SectorPagedResultDTO object.</returns>
    Task<SectorPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);

    /// <summary>
    /// Asynchronously retrieves the total number of sectors that match the given search criteria.
    /// </summary>
    /// <param name="searchString">
    /// A keyword used to filter the sectors by name or other relevant fields. If <c>null</c> or empty, all sectors are counted.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the total number of matching sectors as an integer.
    /// </returns>
    Task<int> GetTotalSectorsCountAsync(string? searchString);

    /// <summary>
    /// Clears all cached total sector counts.
    /// </summary>
    void ClearTotalSectorsCountCache();

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