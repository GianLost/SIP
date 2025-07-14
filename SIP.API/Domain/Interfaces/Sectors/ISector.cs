using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.Entities.Sectors;

namespace SIP.API.Domain.Interfaces.Sectors;

/// <summary>
/// Provides an abstraction for CRUD operations on sector entities.
/// </summary>
public interface ISector
{
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
    /// Retorna setores paginados, com filtro, ordenação e total de registros.
    /// </summary>
    /// <param name="pageNumber">Número da página (iniciando em 1).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <param name="sortLabel">Campo para ordenação.</param>
    /// <param name="sortDirection">Direção da ordenação ("asc" ou "desc").</param>
    /// <param name="searchString">Texto para filtro de pesquisa.</param>
    /// <returns>Objeto SectorPagedResultDTO.</returns>
    Task<SectorPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);

    /// <summary>
    /// Asynchronously retrieves a paginated list of sectors, optionally sorted and filtered.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The field name used to sort the results (optional).</param>
    /// <param name="sortDirection">
    /// The sort direction: "asc" for ascending or "desc" for descending order (optional).
    /// </param>
    /// <param name="searchString">
    /// A keyword used to filter the sectors by name or other relevant fields (optional).
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of <see cref="Sector"/> entities
    /// that match the given pagination, sorting, and filtering criteria.
    /// </returns>
    Task<List<Sector>> GetAllAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);

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