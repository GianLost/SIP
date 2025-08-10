using Microsoft.AspNetCore.Mvc;
using SIP.API.Infrastructure.Caching;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Controllers.Cache;

[Route("sip_api/[controller]")]
[ApiController]
public class CacheController(EntityCacheManager cache) : ControllerBase
{
    private readonly EntityCacheManager _cache = cache;

    /// <summary>
    /// Invalidates the cache for a specific entity.
    /// </summary>
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
    /// Queries the cache state of an entity.
    /// </summary>
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

public record CacheResponse(string Message);
public record CacheStatusResponse(string EntityType, string Status);
public record ErrorResponse(string Error);