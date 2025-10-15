using SIP.UI.Domain.DTOs.Protocols;
using SIP.UI.Domain.DTOs.Protocols.Pagination;
using SIP.UI.Domain.DTOs.Protocols.Response;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Errors;
using SIP.UI.Models.Protocols;
using System.Net.Http.Json;

namespace SIP.UI.Domain.Services.Protocols;

public class ProtocolService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task<ProtocolResponseDTO?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ProtocolResponseDTO>($"{BaseEndpoints<Protocol>._getById}{id}");
        }
        catch
        {
            return null;
        }

    }

    public async Task<ProtocolPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, 100);

        string url = $"{BaseEndpoints<Protocol>._getPaged}pageNumber={pageNumber}&pageSize={pageSize}&sortLabel={sortLabel}&sortDirection={sortDirection}&searchString={searchString}";

        ProtocolPagedResultDTO? response = await _http.GetFromJsonAsync<ProtocolPagedResultDTO>(url);

        return response ?? new ProtocolPagedResultDTO();
    }

    public async Task CreateAsync(ProtocolCreateDTO protocol)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync(BaseEndpoints<Protocol>._create, protocol);
        response.EnsureSuccessStatusCode();
        await InvalidateCacheAsync();
        //// Mapeia o objeto Protocol para o DTO de criação
        //ProtocolCreateDTO protocolCreateDto = new()
        //{
        //    Subject = protocol.Subject,
        //    Description = protocol.Description,
        //    OriginSectorId = protocol.OriginSectorId,
        //    CreatedById = protocol.CreatedById,
        //    DestinationSectorId = protocol.DestinationSectorId,
        //    DestinationUserId = protocol.DestinationUserId,
        //    Status = protocol.Status,
        //    IsArchived = protocol.IsArchived
        //};
    }

    public async Task UpdateAsync(ProtocolUpdateDTO protocol)
    {
        HttpResponseMessage response = await _http.PatchAsJsonAsync($"{BaseEndpoints<Protocol>._update}{protocol.Id}", protocol);
        response.EnsureSuccessStatusCode();
        await InvalidateCacheAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        HttpResponseMessage response = await _http.DeleteAsync($"{BaseEndpoints<Protocol>._delete}{id}");

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
                throw new InvalidOperationException("Protocolo não encontrado.");
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
        string url = CacheEndpoints._invalidateProtocolCount;
        HttpResponseMessage response = await _http.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
    }
}