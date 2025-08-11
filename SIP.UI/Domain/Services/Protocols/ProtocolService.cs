using SIP.UI.Domain.DTOs.Protocols.Responses;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Protocols;
using System.Net.Http.Json;

namespace SIP.UI.Domain.Services.Protocols;

public class ProtocolService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task<ProtocolPagedResultDTO> GetPagedProtocolsAsync(int page, int pageSize, string sortLabel, string sortDirection, string search)
    {
        var url = $"sip_api/Protocol/show_paged?page={page}&pageSize={pageSize}&sort={sortLabel}&direction={sortDirection}&search={search}";
        return await _http.GetFromJsonAsync<ProtocolPagedResultDTO>(url) ?? new ProtocolPagedResultDTO();
    }

    public async Task<int> GetTotalProtocolsCountAsync(string search)
    {
        var url = $"sip_api/Protocol/count?search={search}";
        return await _http.GetFromJsonAsync<int>(url);
    }

    public async Task CreateProtocolAsync(Protocol protocol)
    {
        await _http.PostAsJsonAsync("sip_api/Protocol/register_protocol", protocol);
    }

    public async Task UpdateProtocolAsync(Protocol protocol)
    {
        await _http.PutAsJsonAsync($"sip_api/Protocol/update_protocol/{protocol.Id}", protocol);
    }

    public async Task DeleteProtocolAsync(Guid id)
    {
        HttpResponseMessage response = await _http.DeleteAsync($"sip_api/Protocol/delete/{id}");

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                try
                {
                    ErrorResponse? errorObject = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent);

                    throw new InvalidOperationException(errorObject?.Error ?? "Erro desconhecido ao excluir protocolo.");
                }
                catch (System.Text.Json.JsonException)
                {
                    throw new InvalidOperationException($"Erro de formato ao excluir protocolo: {errorContent}");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("protocolo não encontrada.");
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