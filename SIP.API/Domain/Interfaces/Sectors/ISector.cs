using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Interfaces.Default;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;
using SIP.API.Domain.DTOs.Users.Pagination;
using SIP.API.Domain.DTOs.Default.Pagination;

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

    /// <summary>
    /// Retrieves a paginated list of sectors for selection purposes.
    /// </summary>
    /// <param name="skip">
    /// The number of records to skip before starting to return results.  
    /// Used for pagination control.
    /// </param>
    /// <param name="take">
    /// The maximum number of records to return in the result set.  
    /// Defines the page size of the query.
    /// </param>
    /// <param name="searchString">
    /// An optional search term used to filter sectors by name, acronym, or other identifying attributes.  
    /// If null or empty, all sectors are returned based on the specified pagination parameters.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a <see cref="PagedResultDTO{SectorBasicListDTO}"/>  
    /// with the list of sectors that match the given criteria and pagination settings.
    /// </returns>
    /// <remarks>
    /// This method is typically used to populate dropdowns or autocomplete fields where the user must
    /// select a sector from a large dataset.  
    /// 
    /// - Supports pagination through the <paramref name="skip"/> and <paramref name="take"/> parameters.  
    /// - Allows optional text-based filtering using <paramref name="searchString"/>.  
    /// - Returns an empty result set if no sectors match the provided criteria.  
    /// 
    /// The data returned usually contains basic identifying information about each sector, such as
    /// its ID, name, and acronym, for efficient display in user interfaces.
    /// </remarks>
    /// <exception cref="System.Exception">
    /// Thrown when an unexpected error occurs while retrieving the sectors from the database.
    /// </exception>
    Task<PagedResultDTO<SectorBasicListDTO>> GetSectorsToSelection(int skip = 0, int take = 15, string? searchString = null);
}