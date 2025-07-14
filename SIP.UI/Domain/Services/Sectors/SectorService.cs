using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Sectors;
using System.Net.Http.Json;

namespace SIP.UI.Domain.Services.Sectors;

public class SectorService(HttpClient http)
{
    private readonly HttpClient _http = http;

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

    public async Task<Sector?> GetSetorAsync(Guid id)
        => await _http.GetFromJsonAsync<Sector>($"{SectorsEndpoints._getSectorsById}{id}");

    public async Task CreateSectorAsync(Sector setor)
    {
        var response = await _http.PostAsJsonAsync(SectorsEndpoints._createSector, setor);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateSectorAsync(Sector setor)
    {
        var response = await _http.PutAsJsonAsync($"{SectorsEndpoints._updateSector}{setor.Id}", setor);
        response.EnsureSuccessStatusCode();
    }

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

public class ErrorResponse
{
    public string? Error { get; set; }
}