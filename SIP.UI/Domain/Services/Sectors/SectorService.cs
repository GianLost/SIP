using System.Text.Json;
using System.Net.Http.Json;
using SIP.UI.Models.Errors;
using SIP.UI.Models.Sectors;
using SIP.UI.Domain.DTOs.Sectors;
using SIP.UI.Domain.DTOs.Default.Pagination;
using SIP.UI.Domain.DTOs.Sectors.Pagination;
using SIP.UI.Domain.DTOs.Sectors.Request;
using SIP.UI.Domain.Helpers.Endpoints;

namespace SIP.UI.Domain.Services.Sectors;

public class SectorService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task CreateAsync(SectorCreateDTO setor)
    {
        HttpResponseMessage request = 
            await _http.PostAsJsonAsync(
                requestUri: BaseEndpoints<Sector>._create, 
                value: setor);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task<PagedResultDTO<SectorRequestDTO>?> GetByIdAsync(Guid id)
    {
        try
        {
            return 
                await _http.GetFromJsonAsync<PagedResultDTO<SectorRequestDTO>>(
                    requestUri: $"{BaseEndpoints<Sector>._getById}{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<PagedResultDTO<SectorDefaultRequestDTO>?> GetAllAsync()
    {
        try
        {
            string endpoint = BaseEndpoints<Sector>._getAll;

            var request = 
                await _http.GetFromJsonAsync<PagedResultDTO<SectorDefaultRequestDTO>>(
                    requestUri: endpoint);

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

        string url = $"{BaseEndpoints<Sector>._getPaged}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        var request = 
            await _http.GetFromJsonAsync<PagedResultDTO<SectorListItemDTO>>(
                requestUri: url);

        return request ?? new PagedResultDTO<SectorListItemDTO>();
    }

    public async Task UpdateAsync(SectorUpdateDTO setor)
    {
        HttpResponseMessage request = 
            await _http.PatchAsJsonAsync(
                requestUri: $"{BaseEndpoints<Sector>._update}{setor.Id}", 
                value: setor);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        HttpResponseMessage request = 
            await _http.DeleteAsync(
                requestUri: $"{BaseEndpoints<Sector>._delete}{id}");

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
        string url = CacheEndpoints._invalidateSectorCount;

        HttpResponseMessage request = 
            await 
            _http.PostAsync(
                requestUri: url,content: null);

        request.EnsureSuccessStatusCode();
    }
}