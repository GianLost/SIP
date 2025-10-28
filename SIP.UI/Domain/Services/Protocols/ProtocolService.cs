using SIP.UI.Domain.DTOs.Protocols;
using SIP.UI.Domain.DTOs.Protocols.Pagination;
using SIP.UI.Domain.DTOs.Protocols.Response;
using SIP.UI.Domain.Helpers.Endpoints;
using SIP.UI.Models.Errors;
using SIP.UI.Models.Protocols;
using System.Net.Http.Json;
using System.Text.Json;

namespace SIP.UI.Domain.Services.Protocols;

public class ProtocolService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task CreateAsync(ProtocolCreateDTO protocol)
    {
        HttpResponseMessage request = 
            await _http.PostAsJsonAsync(
                requestUri: BaseEndpoints<Protocol>._create, 
                value: protocol);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task<ProtocolResponseDTO?> GetByIdAsync(Guid id)
    {
        try
        {
            return 
                await _http.GetFromJsonAsync<ProtocolResponseDTO>(
                    requestUri: $"{BaseEndpoints<Protocol>._getById}{id}");
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

        ProtocolPagedResultDTO? request = 
            await _http.GetFromJsonAsync<ProtocolPagedResultDTO>(
                requestUri: url);

        return request ?? new ProtocolPagedResultDTO();
    }

    public async Task UpdateAsync(ProtocolUpdateDTO protocol)
    {
        HttpResponseMessage request = 
            await _http.PatchAsJsonAsync(
                requestUri: $"{BaseEndpoints<Protocol>._update}{protocol.Id}", 
                value: protocol);

        request.EnsureSuccessStatusCode();

        await InvalidateCacheAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        HttpResponseMessage request = 
            await _http.DeleteAsync(
                requestUri: $"{BaseEndpoints<Protocol>._delete}{id}");

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

                    throw new InvalidOperationException(errorObject?.Error ?? "Erro desconhecido ao excluir protocolo.");
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException($"Erro de formato ao excluir protocolo: {errorContent}");
                }
            }
            else if (request.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Protocolo não encontrado.");
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
        string url = CacheEndpoints._invalidateProtocolCount;

        HttpResponseMessage response = 
            await _http.PostAsync(
                requestUri: url, 
                content: null);

        response.EnsureSuccessStatusCode();
    }
}