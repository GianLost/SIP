using SIP.UI.Domain.DTOs.Default.Pagination;
using SIP.UI.Domain.DTOs.Protocols.Default;
using SIP.UI.Domain.DTOs.Users;
using SIP.UI.Domain.DTOs.Users.Configurations;
using SIP.UI.Domain.DTOs.Users.Pagination;
using SIP.UI.Domain.DTOs.Users.Request;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Errors;
using SIP.UI.Models.Users;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SIP.UI.Domain.Services.Users;

public class UserService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task CreateAsync(UserCreateDTO user)
    {
        HttpResponseMessage request = 
            await _http.PostAsJsonAsync(
                requestUri: BaseEndpoints<User>._create, 
                value: user);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task<PagedResultDTO<UserListItemDTO>?> GetByIdAsync(Guid id)
    {
        try
        {
            return 
                await _http.GetFromJsonAsync<PagedResultDTO<UserListItemDTO>>(
                    requestUri: $"{BaseEndpoints<User>._getById}{id}");
        }
        catch
        {
            return null;
        }

    }

    public async Task<PagedResultDTO<UserRequestDTO>?> GetAllAsync()
    {
        try
        {
            string endpoint = BaseEndpoints<User>._getAll;

            var request =
                await _http.GetFromJsonAsync<PagedResultDTO<UserRequestDTO>>(
                    requestUri: endpoint);

            return request;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Falha ao carregar a lista completa de usuários: {ex.Message}");
            return null;
        }
    }

    public async Task<PagedResultDTO<UserListItemDTO>?> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, 100);

        string url = $"{BaseEndpoints<User>._getPaged}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        var request = 
            await _http.GetFromJsonAsync<PagedResultDTO<UserListItemDTO>>(
                requestUri: url);

        return request ?? new PagedResultDTO<UserListItemDTO>();
    }

    public async Task<List<ProtocolDefaultDTO>> GetCreatedProtocolsByUserAsync(Guid userId)
    {
        string uri = $"{BaseEndpoints<User>._base}/{userId}/protocols_created";
        return await _http.GetFromJsonAsync<List<ProtocolDefaultDTO>>(uri) ?? [];
    }

    public async Task<PagedResultDTO<UserListItemDTO>?> GetDetailsAsync(Guid id)
    {
        try
        {
            string url = $"{BaseEndpoints<User>._getById}{id}/details";
            return await _http.GetFromJsonAsync<PagedResultDTO<UserListItemDTO>>(requestUri: url);
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateAsync(UserUpdateDTO user)
    {
        HttpResponseMessage request = 
            await _http.PatchAsJsonAsync(
                requestUri: $"{BaseEndpoints<User>._update}{user.Id}", 
                value: user);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        HttpResponseMessage request = 
            await _http.DeleteAsync(
                requestUri: $"{BaseEndpoints<User>._delete}{id}");

        if (!request.IsSuccessStatusCode)
        {
            string errorContent = 
                await request.Content.ReadAsStringAsync();

            if (request.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                try
                {
                    ErrorResponse? errorObject = 
                        JsonSerializer.Deserialize<ErrorResponse>(errorContent);

                    throw new InvalidOperationException(errorObject?.Error ?? "Erro desconhecido ao excluir usuário.");
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException($"Erro de formato ao excluir usuário: {errorContent}");
                }
            }
            else if (request.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("usuário não encontrado.");
            }
            else
            {
                throw new HttpRequestException($"Erro na requisição: {request.StatusCode} - {errorContent}");
            }
        }

        await InvalidateCacheAsync();
    }

    public async Task<bool> DefaultChangePasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            AdminResetPasswordDTO changePasswordDto = new()
            {
                Password = newPassword
            };

            HttpResponseMessage request = 
                await _http.PatchAsJsonAsync(
                    requestUri: $"{BaseEndpoints<User>._password}/{userId}",
                    value: changePasswordDto);

            request.EnsureSuccessStatusCode();

            return true;
        }
        catch (HttpRequestException ex)
        {

            throw new Exception($"Falha ao alterar senha do usuário. Detalhes: {ex.Message}");
        }
    }

    public async Task<User?> ChangeUserSectorAsync(UserChangeSectorDTO dto)
    {
        var response = await _http.PatchAsJsonAsync($"{BaseEndpoints<User>._base}/{dto.UserId}/sector", dto);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<User>();

        return null;
    }

    private async Task InvalidateCacheAsync()
    {
        string url = CacheEndpoints._invalidateUserCount;

        HttpResponseMessage response = 
            await _http.PostAsync(
                requestUri: url, 
                content: null);

        response.EnsureSuccessStatusCode();
    }
}