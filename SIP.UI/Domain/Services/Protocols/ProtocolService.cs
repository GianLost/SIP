using SIP.UI.Domain.DTOs.Protocols;
using SIP.UI.Domain.DTOs.Protocols.Responses;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Protocols;
using System.Net;
using System.Net.Http.Json;

namespace SIP.UI.Domain.Services.Protocols;

public class ProtocolService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task<ProtocolPagedResultDTO> GetPagedProtocolsAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, 100);

        string url = $"{ProtocolsEndpoints._protocolsPaginationFull}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        ProtocolPagedResultDTO? response = await _http.GetFromJsonAsync<ProtocolPagedResultDTO>(url);

        return response ?? new ProtocolPagedResultDTO();
    }

    public async Task<Protocol?> GetProtocolByIdAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<Protocol>($"{ProtocolsEndpoints._getProtocolsById}{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<int> GetTotalProtocolsCountAsync(string? searchString = null)
    {
        string url = ProtocolsEndpoints._protocolsCounter;

        if (!string.IsNullOrEmpty(searchString))
            url += $"?searchString={Uri.EscapeDataString(searchString)}";

        try
        {
            int count = await _http.GetFromJsonAsync<int>(url);
            return count;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Falha ao obter o total de protocolos. Detalhes: {ex.Message}");
        }
    }

    public async Task CreateProtocolAsync(Protocol protocol)
    {
        // Mapeia o objeto Protocol para o DTO de criação
        ProtocolCreateDTO protocolCreateDto = new()
        {
            Subject = protocol.Subject,
            Description = protocol.Description,
            OriginSectorId = protocol.OriginSectorId,
            CreatedById = protocol.CreatedById,
            DestinationSectorId = protocol.DestinationSectorId,
            DestinationUserId = protocol.DestinationUserId,
            Status = protocol.Status,
            IsArchived = protocol.IsArchived
        };

        await _http.PostAsJsonAsync(ProtocolsEndpoints._createProtocol, protocolCreateDto);
        await InvalidateSectorCacheAsync();
    }

    public async Task UpdateProtocolAsync(Protocol protocol)
    {
        await _http.PutAsJsonAsync($"sip_api/Protocol/update_protocol/{protocol.Id}", protocol);
        await InvalidateSectorCacheAsync();
    }

    public async Task DeleteProtocolAsync(Guid id)
    {
        HttpResponseMessage response = await _http.DeleteAsync($"{ProtocolsEndpoints._deleteProtocol}{id}");

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new InvalidOperationException(error?.Error ?? "Erro ao excluir protocolo.");
        }
        response.EnsureSuccessStatusCode();

        await InvalidateSectorCacheAsync();
    }

    private async Task InvalidateSectorCacheAsync()
    {
        string url = CacheEndpoints._invalidateSectorCount;
        HttpResponseMessage response = await _http.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
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