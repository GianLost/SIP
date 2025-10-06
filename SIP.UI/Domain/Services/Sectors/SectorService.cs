using SIP.UI.Domain.DTOs.Sectors;
using SIP.UI.Domain.DTOs.Sectors.Default;
using SIP.UI.Domain.DTOs.Sectors.Pagination;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Errors;
using SIP.UI.Models.Sectors;
using System.Net.Http.Json;

namespace SIP.UI.Domain.Services.Sectors;

/// <summary>
/// Service for interacting with the Sector API endpoints.
/// Initializes a new instance of the <see cref="SectorService"/> class.
/// </summary>
/// <param name="http">The HTTP client used for API requests.</param>
public class SectorService(HttpClient http)
{
    private readonly HttpClient _http = http;

    /// <summary>
    /// Gets a sector by its unique identifier from the API.
    /// </summary>
    /// <param name="id">The unique identifier of the sector.</param>
    /// <returns>The sector entity if found; otherwise, null.</returns>
    public async Task<Sector?> GetByIdAsync(Guid id) => 
        await _http.GetFromJsonAsync<Sector>($"{BaseEndpoints<Sector>._getById}{id}");

    /// <summary>
    /// Busca TODOS os setores da API para usar em dropdowns e seletores.
    /// </summary>
    /// <returns>Uma lista completa de todos os setores.</returns>
    public async Task<ICollection<SectorDefaultDTO>?> GetAllAsync()
    {
        try
        {
            string endpoint = BaseEndpoints<Sector>._getAll;

            ICollection<SectorDefaultDTO>? sectors = await _http.GetFromJsonAsync<ICollection<SectorDefaultDTO>>(endpoint);

            return sectors ?? [];
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Falha ao carregar a lista completa de setores: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets a paginated result of sectors from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page (limited to 100).</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A paged result DTO containing the sectors and total count.</returns>
    public async Task<SectorPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, 100);

        string url = $"{BaseEndpoints<Sector>._getPaged}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        SectorPagedResultDTO? response = await _http.GetFromJsonAsync<SectorPagedResultDTO>(url);

        return response ?? new SectorPagedResultDTO();
    }

    /// <summary>
    /// Creates a new sector via the API.
    /// </summary>
    /// <param name="setor">The sector entity to create.</param>
    public async Task CreateAsync(SectorCreateDTO setor)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync(BaseEndpoints<Sector>._create, setor);
        response.EnsureSuccessStatusCode();
        await InvalidateCacheAsync();
    }

    /// <summary>
    /// Updates an existing sector via the API.
    /// </summary>
    /// <param name="setor">The sector entity to update.</param>
    public async Task UpdateAsync(SectorUpdateDTO setor)
    {
        HttpResponseMessage response = await _http.PatchAsJsonAsync($"{BaseEndpoints<Sector>._update}{setor.Id}", setor);
        response.EnsureSuccessStatusCode();
        await InvalidateCacheAsync();
    }

    /// <summary>
    /// Deletes a sector by its unique identifier via the API.
    /// </summary>
    /// <param name="id">The unique identifier of the sector to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown if the sector cannot be deleted due to business rules.</exception>
    /// <exception cref="HttpRequestException">Thrown if the request fails.</exception>
    public async Task DeleteAsync(Guid id)
    {
        HttpResponseMessage response = await _http.DeleteAsync($"{BaseEndpoints<Sector>._delete}{id}");

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                try
                {
                    ErrorResponse? errorObject = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent);

                    throw new InvalidOperationException(errorObject?.Error ?? "Erro desconhecido ao excluir secretaria.");
                }
                catch (System.Text.Json.JsonException)
                {
                    throw new InvalidOperationException($"Erro de formato ao excluir secretaria: {errorContent}");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Secretaria não encontrada.");
            }
            else
            {
                throw new HttpRequestException($"Erro na requisição: {response.StatusCode} - {errorContent}");
            }
        }
        await InvalidateCacheAsync();
    }

    private async Task InvalidateCacheAsync()
    {
        string url = CacheEndpoints._invalidateSectorCount;
        HttpResponseMessage response = await _http.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
    }
}