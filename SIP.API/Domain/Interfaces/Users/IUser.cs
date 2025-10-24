using SIP.API.Domain.DTOs.Default.Pagination;
using SIP.API.Domain.DTOs.Protocols.Default;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Pagination;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Default;

namespace SIP.API.Domain.Interfaces.Users;

/// <summary>
/// Defines a specialized entity manager for user-related operations,
/// extending the generic <see cref="IEntityManager{TEntity, TCreateDTO, TUpdateDTO, TListItemDTO, TResponseDTO}"/>.
/// </summary>
/// <remarks>
/// This interface encapsulates all CRUD, pagination, and management operations
/// for <see cref="User"/> entities within the system.  
/// Additional domain-specific operations (e.g., authentication, password management)
/// can be added here in the future.
/// </remarks>
public interface IUser : IEntityManager<User, UserCreateDTO, UserUpdateDTO, UserBasicListDTO, UserResponseDTO> 
{
    /// <summary>
    /// Retrieves the list of protocols created by a specific user.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier (<see cref="Guid"/>) of the user whose created protocols will be retrieved.
    /// </param>
    /// <returns>
    /// A collection of <see cref="ProtocolDefaultDTO"/> objects representing the protocols created by the user.
    /// </returns>
    /// <remarks>
    /// <b>Purpose:</b>  
    /// Enables tracking of protocol authorship and associations between users and their created records.
    /// 
    /// This method is commonly used in administrative dashboards or auditing modules
    /// where protocol provenance needs to be displayed.
    /// </remarks>
    Task<List<ProtocolDefaultDTO>> GetCreatedProtocolsByUserAsync(Guid userId);

    /// <summary>
    /// Retrieves the detailed information of a specific user.
    /// </summary>
    /// <param name="id">
    /// The unique identifier (<see cref="Guid"/>) of the user whose details will be retrieved.
    /// </param>
    /// <returns>
    /// A <see cref="PagedResultDTO{T}"/> containing a single <see cref="UserListItemDTO"/> element
    /// representing the detailed data of the user.
    /// </returns>
    /// <remarks>
    /// <b>Purpose:</b>  
    /// Provides full inspection of a user’s data, including related roles, sectors,
    /// and metadata for administrative and audit operations.
    /// 
    /// This method differs from list-oriented operations by returning
    /// a single detailed user wrapped in a paged structure for consistency with other endpoints.
    /// </remarks>
    Task<PagedResultDTO<UserListItemDTO>> GetDetailsAsync(Guid id);
}