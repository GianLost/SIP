using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Interfaces.Default;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;
using SIP.API.Domain.DTOs.Users.Pagination;

namespace SIP.API.Domain.Interfaces.Sectors;

/// <summary>
/// Defines a specialized entity manager for sector-related operations,
/// extending the generic <see cref="IEntityManager{TEntity, TCreateDTO, TUpdateDTO, TListItemDTO, TResponseDTO}"/>.
/// </summary>
/// <remarks>
/// This interface provides CRUD, pagination, and data retrieval operations
/// specific to <see cref="Sector"/> entities.  
/// It enables separation of concerns by isolating domain-specific logic
/// from the generic data access layer.
/// </remarks>
public interface ISector : IEntityManager<Sector, SectorCreateDTO, SectorUpdateDTO, SectorListItemDTO, SectorResponseDTO>
{
    /// <summary>
    /// Retrieves all users associated with a specific sector.
    /// </summary>
    /// <param name="sectorId">The unique identifier (GUID) of the sector.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a list of 
    /// <see cref="UserBasicListDTO"/> objects that represent the users linked to the specified sector.
    /// </returns>
    /// <remarks>
    /// This method is responsible for returning all users that belong to the sector
    /// identified by <paramref name="sectorId"/>.  
    /// It is typically used for administrative purposes, such as viewing and managing
    /// users assigned to a specific department or organizational unit.
    ///
    /// - Returns an empty list if no users are associated with the sector.  
    /// - May throw exceptions if a database access error or unexpected runtime issue occurs.  
    /// </remarks>
    /// <exception cref="System.Exception">
    /// Thrown when an unexpected error occurs while retrieving the users of the sector.
    /// </exception>
    Task<List<UserBasicListDTO>> GetUsersBySectorAsync(Guid sectorId);
}