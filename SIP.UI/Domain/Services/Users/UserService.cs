using SIP.UI.Domain.DTOs.Users;
using SIP.UI.Domain.DTOs.Users.Configurations;
using SIP.UI.Domain.DTOs.Users.Pagination;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Users;
using System.Net.Http.Json;

namespace SIP.UI.Domain.Services.Users;

public class UserService(HttpClient http)
{
    private readonly HttpClient _http = http;

    /// <summary>
    /// Gets a paginated result of users from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page (limited to 100).</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A paged result DTO containing the users and total count.</returns>
    public async Task<UserPagedResultDTO> GetPagedUsersAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, 100);

        string url = $"{BaseEndpoints<User>._getPaged}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        UserPagedResultDTO? response = await _http.GetFromJsonAsync<UserPagedResultDTO>(url);

        return response ?? new UserPagedResultDTO();
    }

    /// <summary>
    /// Gets a user by its unique identifier from the API.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    public async Task<User?> GetUsersByIdAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<User>($"{BaseEndpoints<User>._getById}{id}");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Busca TODOS os usuários da API para usar em dropdowns e seletores.
    /// </summary>
    /// <returns>Uma lista completa de todos os usuários.</returns>
    public async Task<ICollection<User>?> GetAllUsersToDropdownAsync()
    {
        try
        {
            string endpoint = BaseEndpoints<User>._getAll;

            ICollection<User>? users = await _http.GetFromJsonAsync<ICollection<User>>(endpoint);

            return users ?? [];
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Falha ao carregar a lista completa de usuários: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new User via the API.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    public async Task CreateUserAsync(UserCreateDTO user)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync(BaseEndpoints<User>._create, user);
        response.EnsureSuccessStatusCode();

        await InvalidateUserCacheAsync();
    }

    /// <summary>
    /// Updates an existing user via the API.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    public async Task UpdateUserAsync(UserUpdateDTO user)
    {
        HttpResponseMessage response = await _http.PatchAsJsonAsync($"{BaseEndpoints<User>._update}{user.Id}", user);
        response.EnsureSuccessStatusCode();
        await InvalidateUserCacheAsync();
    }

    /// <summary>
    /// Deletes a user by its unique identifier via the API.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown if the user cannot be deleted due to business rules.</exception>
    /// <exception cref="HttpRequestException">Thrown if the request fails.</exception>
    public async Task DeleteUserAsync(Guid id)
    {
        HttpResponseMessage response = await _http.DeleteAsync($"{BaseEndpoints<User>._delete}{id}");

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                try
                {
                    ErrorResponse? errorObject = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent);

                    throw new InvalidOperationException(errorObject?.Error ?? "Erro desconhecido ao excluir usuário.");
                }
                catch (System.Text.Json.JsonException)
                {
                    throw new InvalidOperationException($"Erro de formato ao excluir usuário: {errorContent}");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("usuário não encontrado.");
            }
            else
            {
                throw new HttpRequestException($"Erro na requisição: {response.StatusCode} - {errorContent}");
            }
        }

        await InvalidateUserCacheAsync();
    }

    /// <summary>
    /// Altera a senha de um usuário através da API.
    /// </summary>
    /// <param name="userId">O ID do usuário cuja senha será alterada.</param>
    /// <param name="newPassword">A nova senha.</param>
    /// <returns>True se a senha foi alterada com sucesso, caso contrário, false.</returns>
    public async Task<bool> DefaultChangePasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            UserDefaultChangePasswordDTO changePasswordDto = new()
            {
                Id = userId,
                Password = newPassword
            };

            HttpResponseMessage response = await _http.PatchAsJsonAsync(BaseEndpoints<User>._password, changePasswordDto);

            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (HttpRequestException ex)
        {

            throw new Exception($"Falha ao alterar senha do usuário. Detalhes: {ex.Message}");
        }
    }

    private async Task InvalidateUserCacheAsync()
    {
        string url = CacheEndpoints._invalidateUserCount;
        HttpResponseMessage response = await _http.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
    }
}

public class ErrorResponse
{
    /// <summary>
    /// The error message returned by the API.
    /// </summary>
    public string? Error { get; set; }
}