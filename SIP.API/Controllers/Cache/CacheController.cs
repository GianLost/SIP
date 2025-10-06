using Microsoft.AspNetCore.Mvc;
using SIP.API.Controllers.Errors;
using SIP.API.Infrastructure.Caching;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Controllers.Cache;

/// <summary>
/// Controlador responsável pelo gerenciamento do cache de entidades do sistema,
/// incluindo Sectors, Users e Protocols.
/// </summary>
/// <remarks>
/// Este controlador permite invalidar manualmente o cache de entidades específicas,
/// além de consultar o estado atual do cache.
/// </remarks>
[Route("sip_api/cache")]
[ApiController]
public class CacheController(EntityCacheManager cache) : ControllerBase
{
    private readonly EntityCacheManager _cache = cache;

    /// <summary>
    /// Invalida o cache de uma entidade específica (Sectors, Users ou Protocols).
    /// </summary>
    /// <param name="entityType">
    /// O nome da entidade cujo cache deve ser invalidado. 
    /// Valores aceitos: <c>sectors</c>, <c>users</c> ou <c>protocols</c>.
    /// </param>
    /// <returns>
    /// Retorna uma mensagem de confirmação em caso de sucesso ou uma resposta de erro apropriada.
    /// </returns>
    /// <response code="200">Cache da entidade especificada invalidado com sucesso.</response>
    /// <response code="400">O parâmetro <c>entityType</c> é inválido ou não informado.</response>
    /// <response code="500">Erro interno ao tentar invalidar o cache.</response>
    [HttpPost("invalidate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CacheResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public IActionResult Invalidate([FromQuery, Required] string entityType)
    {
        entityType = entityType.Trim();

        if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest(new ErrorResponse("entityType is required."));

        try
        {
            _cache.Invalidate(entityType);
            return Ok(new CacheResponse($"Cache for '{entityType}' invalidated."));
        }
        catch (Exception ex)
        {
            // Logar exceção aqui (ILogger ou outra ferramenta de logging)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse($"An error occurred while invalidating cache: {ex.Message}"));
        }
    }

    /// <summary>
    /// Consulta o estado atual do cache de uma entidade específica (Sectors, Users ou Protocols).
    /// </summary>
    /// <param name="entityType">
    /// O nome da entidade cujo estado do cache será consultado. 
    /// Valores aceitos: <c>sectors</c>, <c>users</c> ou <c>protocols</c>.
    /// </param>
    /// <returns>
    /// Retorna o estado atual do cache da entidade especificada.
    /// </returns>
    /// <response code="200">Estado do cache retornado com sucesso.</response>
    /// <response code="400">O parâmetro <c>entityType</c> é inválido ou não informado.</response>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CacheStatusResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public IActionResult Status([FromQuery, Required] string entityType)
    {
        entityType = entityType.Trim();

        if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest(new ErrorResponse("entityType is required."));

        string status = "Cache status not implemented.";
        return Ok(new CacheStatusResponse(entityType, status));
    }
}

/// <summary>
/// Representa uma resposta genérica de operações de cache.
/// </summary>
/// <param name="Message">Mensagem informativa sobre o resultado da operação.</param>
public record CacheResponse(string Message);

/// <summary>
/// Representa o estado atual do cache de uma entidade.
/// </summary>
/// <param name="EntityType">Nome da entidade consultada.</param>
/// <param name="Status">Descrição textual do estado do cache.</param>
public record CacheStatusResponse(string EntityType, string Status);