using System.Text.Json;
using System.Net.Http.Json;
using SIP.UI.Models.Errors;
using SIP.UI.Models.Sectors;
using SIP.UI.Domain.DTOs.Sectors;
using SIP.UI.Domain.DTOs.Default.Pagination;
using SIP.UI.Domain.DTOs.Sectors.Pagination;
using SIP.UI.Domain.DTOs.Sectors.Request;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Domain.DTOs.Users.Pagination;

namespace SIP.UI.Domain.Services.Sectors;

public class SectorService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task CreateAsync(SectorCreateDTO setor)
    {
        string uri = BaseEndpoints<Sector>._create;

        HttpResponseMessage request = 
            await _http.PostAsJsonAsync(
                requestUri: uri,
                value: setor);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task<PagedResultDTO<SectorListItemDTO>?> GetByIdAsync(Guid id)
    {
        try
        {
            string uri = $"{BaseEndpoints<Sector>._getById}{id}";

            return 
                await _http.GetFromJsonAsync<PagedResultDTO<SectorListItemDTO>>(
                    requestUri: uri);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PagedResultDTO<SectorRequestDTO>?> GetAllAsync()
    {
        try
        {
            string uri = BaseEndpoints<Sector>._getAll;

            var request = 
                await _http.GetFromJsonAsync<PagedResultDTO<SectorRequestDTO>>(
                    requestUri: uri);

            return request;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Falha ao carregar a lista completa de setores: {ex.Message}");
            return null;
        }
    }

    public async Task<PagedResultDTO<SectorListItemDTO>?> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, 100);

        string uri = $"{BaseEndpoints<Sector>._getPaged}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        var request = 
            await _http.GetFromJsonAsync<PagedResultDTO<SectorListItemDTO>>(
                requestUri: uri);

        return request ?? new PagedResultDTO<SectorListItemDTO>();
    }

    public async Task<List<UserBasicListDTO>> GetUsersBySectorAsync(Guid sectorId)
    {
        string uri = $"{BaseEndpoints<Sector>._base}/{sectorId}/users";
        return await _http.GetFromJsonAsync<List<UserBasicListDTO>>(uri) ?? [];
    }

    public async Task UpdateAsync(SectorUpdateDTO setor)
    {
        string uri = $"{BaseEndpoints<Sector>._update}{setor.Id}";
        HttpResponseMessage request = 
            await _http.PatchAsJsonAsync(
                requestUri: uri, 
                value: setor);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        string uri = $"{BaseEndpoints<Sector>._delete}{id}";

        HttpResponseMessage request = 
            await _http.DeleteAsync(
                requestUri: uri);

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

                    throw new InvalidOperationException(errorObject?.Error ?? "Erro desconhecido ao excluir secretaria.");
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException($"Erro de formato ao excluir secretaria: {errorContent}");
                }
            }
            else if (request.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Secretaria não encontrada.");
            }
            else
            {
                throw new HttpRequestException($"Erro na requisição: {request.StatusCode} - {errorContent}");
            }
        }
        await InvalidateCacheAsync();
    }

    private async Task InvalidateCacheAsync()
    {
        string uri = CacheEndpoints._invalidateSectorCount;

        HttpResponseMessage request = 
            await 
            _http.PostAsync(
                requestUri: uri,content: null);

        request.EnsureSuccessStatusCode();
    }
}