using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.Interfaces.Users;

/// <summary>
/// Provides an abstraction for CRUD operations on user entities.
/// </summary>
public interface IUser
{
    /// <summary>
    /// Asynchronously creates a new user in the database.
    /// </summary>
    /// <param name="dto">A data transfer object containing the user's information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the created <see cref="User"/> entity.
    /// </returns>
    Task<User> CreateAsync(UserCreateDTO dto);

    /// <summary>
    /// Asynchronously retrieves a user by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="User"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Asynchronously retrieves a paginated list of users from the database.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of <see cref="User"/> entities.
    /// </returns>
    Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Asynchronously updates an existing user in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="dto">A data transfer object containing the updated user values.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the updated <see cref="User"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> UpdateAsync(Guid id, UserUpdateDTO dto);

    /// <summary>
    /// Asynchronously deletes a user by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if the user was deleted; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> DeleteAsync(Guid id);
}