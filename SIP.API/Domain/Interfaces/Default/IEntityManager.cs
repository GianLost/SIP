using SIP.API.Domain.DTOs.Default.Pagination;

namespace SIP.API.Domain.Interfaces.Default;

/// <summary>
/// Defines a generic contract for managing entities within the system,
/// providing a consistent abstraction for CRUD operations, pagination,
/// and cache management. 
/// </summary>
/// <typeparam name="TEntity">The underlying entity type (domain model) managed by the service.</typeparam>
/// <typeparam name="TCreateDTO">The DTO used to create new entity instances.</typeparam>
/// <typeparam name="TUpdateDTO">The DTO used to update existing entity instances.</typeparam>
/// <typeparam name="TListItemDTO">The DTO used to represent lightweight entity data in list or table views.</typeparam>
/// <typeparam name="TResponseDTO">The DTO used for detailed responses that represent the complete entity data.</typeparam>
public interface IEntityManager<TEntity, TCreateDTO, TUpdateDTO, TListItemDTO, TResponseDTO>
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : class
    where TListItemDTO : class
    where TResponseDTO : class
{
    /// <summary>
    /// Creates a new entity based on the provided data transfer object (DTO).
    /// </summary>
    /// <param name="dto">The DTO containing data for the new entity.</param>
    /// <returns>The created entity instance.</returns>
    /// <remarks>
    /// This method should persist the entity to the database and return
    /// the fully tracked or detached entity, depending on the implementation.
    /// </remarks>
    Task<TEntity> CreateAsync(TCreateDTO dto);

    /// <summary>
    /// Retrieves a specific entity by its unique identifier, returning its detailed representation.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>
    /// A paged result containing the detailed DTO representation (<typeparamref name="TListItemDTO"/>).
    /// </returns>
    /// <remarks>
    /// Typically used for detailed entity views or inspection screens.
    /// </remarks>
    Task<PagedResultDTO<TListItemDTO>> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a specific entity by its unique identifier, returning a simplified representation.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>
    /// A paged result containing a simplified DTO representation (<typeparamref name="TResponseDTO"/>).
    /// </returns>
    /// <remarks>
    /// Often used in dropdowns or nested data structures where a full representation is unnecessary.
    /// </remarks>
    Task<PagedResultDTO<TResponseDTO>> GetByIdDefaultAsync(Guid id);

    /// <summary>
    /// Retrieves all entities in the dataset, returning simplified representations.
    /// </summary>
    /// <returns>
    /// A paged result containing a list of simplified DTOs (<typeparamref name="TResponseDTO"/>).
    /// </returns>
    /// <remarks>
    /// This method may include caching mechanisms for better performance on frequently accessed datasets.
    /// </remarks>
    Task<PagedResultDTO<TResponseDTO>> GetAllAsync();

    /// <summary>
    /// Retrieves a paginated and optionally filtered collection of entities.
    /// </summary>
    /// <param name="pageNumber">The current page number (starting from 1).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortLabel">The field by which results should be sorted.</param>
    /// <param name="sortDirection">The sort order ("asc" or "desc").</param>
    /// <param name="searchString">An optional search filter applied to the dataset.</param>
    /// <returns>
    /// A paged result containing the requested subset of entities, represented by
    /// <typeparamref name="TListItemDTO"/>.
    /// </returns>
    /// <remarks>
    /// This method supports advanced data grid scenarios and UI pagination components.
    /// </remarks>
    Task<PagedResultDTO<TListItemDTO>> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);


    /// <summary>
    /// Updates an existing entity identified by the specified ID using the provided data.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to update.</param>
    /// <param name="dto">The DTO containing the updated entity data.</param>
    /// <returns>The updated entity instance, or null if the entity was not found.</returns>
    /// <remarks>
    /// Implementations should ensure concurrency control and entity validation before applying updates.
    /// </remarks>
    Task<TEntity?> UpdateAsync(Guid id, TUpdateDTO dto);

    /// <summary>
    /// Deletes an existing entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>True if the entity was successfully deleted; otherwise, false.</returns>
    /// <remarks>
    /// This method should handle any integrity constraints and related cleanup.
    /// </remarks>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Retrieves the total count of entities that match the specified search criteria.
    /// </summary>
    /// <param name="searchString">An optional search string to filter the dataset.</param>
    /// <returns>The total number of matching entities.</returns>
    /// <remarks>
    /// Commonly used for pagination calculations and performance optimization through caching.
    /// </remarks>
    Task<int> GetTotalCountAsync(string? searchString);

    /// <summary>
    /// Clears any cached count data associated with this entity type.
    /// </summary>
    /// <remarks>
    /// Should be invoked after create, update, or delete operations to maintain cache consistency.
    /// </remarks>
    void ClearTotalCountCache();
}