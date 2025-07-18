using System.Net.Http.Json;
using SIP.UI.Models.Users;
using SIP.UI.Domain.DTOs.Users;
using SIP.UI.Domain.DTOs.Users.Configurations;

namespace SIP.UI.Domain.Services.Users;

public class UserService(HttpClient http)
{
    private readonly HttpClient _http = http;

    /// <summary>
    /// Creates a new User via the API.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    public async Task CreateUserAsync(User user)
    {
        var response = await _http.PostAsJsonAsync("sip_api/User/register_user", user);
        response.EnsureSuccessStatusCode();

        await InvalidateUserCountCache();
    }

    /// <summary>
    /// Gets a paginated list of users from the API.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A list of users for the specified page, or null if not found.</returns>
    public async Task<List<User>?> GetUsersAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection = null, string? searchString = null)
    {
        var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(sortLabel))
            queryParams.Add($"sortLabel={Uri.EscapeDataString(sortLabel)}");

        if (!string.IsNullOrEmpty(sortDirection))
            queryParams.Add($"sortDirection={Uri.EscapeDataString(sortDirection)}");

        if (!string.IsNullOrEmpty(searchString))
            queryParams.Add($"searchString={Uri.EscapeDataString(searchString)}");

        var url = $"sip_api/User/show?{string.Join("&", queryParams)}";

        try
        {
            var sectors = await _http.GetFromJsonAsync<List<User>>(url);
            return sectors;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Falha ao carregar os usuários. Detalhes: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a user by its unique identifier from the API.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    public async Task<User?> GetUsersAsync(Guid id)
        => await _http.GetFromJsonAsync<User>($"sip_api/User/{id}");

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

        var url = $"sip_api/User/show_paged?pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        var response = await _http.GetFromJsonAsync<UserPagedResultDTO>(url);

        return response ?? new UserPagedResultDTO();
    }

    /// <summary>
    /// Gets the total count of user from the API, optionally filtered by a search string.
    /// </summary>
    /// <param name="searchString">Optional search string to filter users.</param>
    /// <returns>The total number of users matching the filter.</returns>
    public async Task<int> GetTotalUsersCountAsync(string? searchString = null)
    {
        var url = "sip_api/User/count";

        if (!string.IsNullOrEmpty(searchString))
            url += $"?searchString={Uri.EscapeDataString(searchString)}";

        try
        {
            var count = await _http.GetFromJsonAsync<int>(url);
            return count;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Falha ao obter o total de usuários. Detalhes: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing user via the API.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    public async Task UpdateUserAsync(User user)
    {
        var response = await _http.PutAsJsonAsync($"sip_api/User/update_user/{user.Id}", user);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a user by its unique identifier via the API.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown if the user cannot be deleted due to business rules.</exception>
    /// <exception cref="HttpRequestException">Thrown if the request fails.</exception>
    public async Task DeleteUserAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"sip_api/User/delete/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                try
                {
                    var errorObject = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent);

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

        await InvalidateUserCountCache();
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
            var changePasswordDto = new UserDefaultChangePasswordDTO
            {
                UserId = userId,
                Password = newPassword
            };

            var response = await _http.PatchAsJsonAsync("sip_api/User/default-change-password", changePasswordDto);

            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (HttpRequestException ex)
        {

            throw new Exception($"Falha ao alterar senha do usuário. Detalhes: {ex.Message}");
        }
    }

    private async Task InvalidateUserCountCache()
    {
        try
        {
            var response = await _http.PostAsync("sip_api/User/invalidate_count_cache", null);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invalidating user count cache: {ex.Message}");
        }
    }
}

public class ErrorResponse
{
    /// <summary>
    /// The error message returned by the API.
    /// </summary>
    public string? Error { get; set; }
}