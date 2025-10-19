using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Pagination;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Default;

namespace SIP.API.Domain.Interfaces.Users;

/// <summary>
/// Defines a specialized entity manager for user-related operations,
/// extending the generic <see cref="IEntityManager{TEntity, TCreateDTO, TUpdateDTO, TListItemDTO, TDefaultResponse, TResponseDTO}"/>.
/// </summary>
/// <remarks>
/// This interface encapsulates all CRUD, pagination, and management operations
/// for <see cref="User"/> entities within the system.  
/// Additional domain-specific operations (e.g., authentication, password management)
/// can be added here in the future.
/// </remarks>
public interface IUser : IEntityManager<User, UserCreateDTO, UserUpdateDTO, UserListItemDTO, UserDefaultResponseDTO, UserResponseDTO> { }