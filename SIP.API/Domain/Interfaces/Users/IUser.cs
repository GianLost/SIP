using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.Interfaces.Users;

/// <summary>
/// Provides an abstraction for CRUD operations on user entities.
/// </summary>
public interface IUser
{

    /// <summary>
    /// Asynchronously retrieves the total number of users that match the given search criteria.
    /// </summary>
    /// <param name="searchString">
    /// A keyword used to filter the users by name or other relevant fields. If <c>null</c> or empty, all users are counted.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the total number of matching users as an integer.
    /// </returns>
    Task<int> GetTotalUsersCountAsync(string? searchString);

    void ClearTotalUsersCountCache();

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
    /// Retorna usuários paginados, com filtro, ordenação e total de registros.
    /// </summary>
    /// <param name="pageNumber">Número da página (iniciando em 1).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <param name="sortLabel">Campo para ordenação.</param>
    /// <param name="sortDirection">Direção da ordenação ("asc" ou "desc").</param>
    /// <param name="searchString">Texto para filtro de pesquisa.</param>
    /// <returns>Objeto UserPagedResultDTO.</returns>
    Task<UserPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);

    /// <summary>
    /// Asynchronously retrieves a paginated list of users, optionally sorted and filtered.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The field name used to sort the results (optional).</param>
    /// <param name="sortDirection">
    /// The sort direction: "asc" for ascending or "desc" for descending order (optional).
    /// </param>
    /// <param name="searchString">
    /// A keyword used to filter the users by name or other relevant fields (optional).
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of <see cref="User"/> entities
    /// that match the given pagination, sorting, and filtering criteria.
    /// </returns>
    Task<List<User>> GetAllAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString);

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