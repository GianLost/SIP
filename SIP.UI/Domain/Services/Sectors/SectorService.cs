using SIP.UI.Domain.DTOs.Sectors;
using SIP.UI.Domain.Helpers.Endpoints;
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
    /// Gets a paginated list of sectors from the API.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A list of sectors for the specified page, or null if not found.</returns>
    public async Task<List<Sector>?> GetSectorsAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection = null, string? searchString = null)
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

        var url = $"{SectorsEndpoints._sectorsPagination}{string.Join("&", queryParams)}";

        try
        {
            var sectors = await _http.GetFromJsonAsync<List<Sector>>(url);
            return sectors;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Falha ao carregar os setores. Detalhes: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a sector by its unique identifier from the API.
    /// </summary>
    /// <param name="id">The unique identifier of the sector.</param>
    /// <returns>The sector entity if found; otherwise, null.</returns>
    public async Task<Sector?> GetSectorsAsync(Guid id)
        => await _http.GetFromJsonAsync<Sector>($"{SectorsEndpoints._getSectorsById}{id}");

    /// <summary>
    /// Gets a paginated result of sectors from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page (limited to 100).</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A paged result DTO containing the sectors and total count.</returns>
    public async Task<SectorPagedResultDTO> GetPagedSectorsAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, 100);

        var url = $"{SectorsEndpoints._sectorsPaginationFull}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        var response = await _http.GetFromJsonAsync<SectorPagedResultDTO>(url);

        return response ?? new SectorPagedResultDTO();
    }

    /// <summary>
    /// Gets the total count of sectors from the API, optionally filtered by a search string.
    /// </summary>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>The total number of sectors matching the filter.</returns>
    public async Task<int> GetTotalSectorsCountAsync(string? searchString = null)
    {
        var url = SectorsEndpoints._sectorsCounter;

        if (!string.IsNullOrEmpty(searchString))
            url += $"?searchString={Uri.EscapeDataString(searchString)}";

        try
        {
            var count = await _http.GetFromJsonAsync<int>(url);
            return count;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Falha ao obter o total de secretarias. Detalhes: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new sector via the API.
    /// </summary>
    /// <param name="setor">The sector entity to create.</param>
    public async Task CreateSectorAsync(Sector setor)
    {
        var response = await _http.PostAsJsonAsync(SectorsEndpoints._createSector, setor);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Updates an existing sector via the API.
    /// </summary>
    /// <param name="setor">The sector entity to update.</param>
    public async Task UpdateSectorAsync(Sector setor)
    {
        var response = await _http.PutAsJsonAsync($"{SectorsEndpoints._updateSector}{setor.Id}", setor);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a sector by its unique identifier via the API.
    /// </summary>
    /// <param name="id">The unique identifier of the sector to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown if the sector cannot be deleted due to business rules.</exception>
    /// <exception cref="HttpRequestException">Thrown if the request fails.</exception>
    public async Task DeleteSectorAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{SectorsEndpoints._deleteSector}{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                try
                {
                    var errorObject = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent);

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
    }
}

/// <summary>
/// Model for error responses from the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// The error message returned by the API.
    /// </summary>
    public string? Error { get; set; }
}