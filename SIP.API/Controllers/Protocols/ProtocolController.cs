using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.DTOs.Protocols.Responses;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Interfaces.Protocols;

namespace SIP.API.Controllers.Protocols;

[Route("sip_api/[controller]")]
[ApiController]
public class ProtocolController(IProtocol protocol) : ControllerBase
{
    private readonly IProtocol _protocolService = protocol;

    /// <summary>
    /// Registers a new protocol in the system.
    /// </summary>
    /// <param name="protocolDTO">The data transfer object containing the protocol's information.</param>
    [HttpPost("register_protocol")]
    [ProducesResponseType(typeof(Protocol), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] ProtocolCreateDTO protocolDTO)
    {
        try
        {
            Protocol entity = 
                await _protocolService.CreateAsync(protocolDTO);

            ProtocolResponseDTO response = new()
            {
                Id = entity.Id,
                Number = entity.Number,
                Subject = entity.Subject,
                Description = entity.Description,
                Status = entity.Status,
                IsArchived = entity.IsArchived,
                CreatedByName = entity.CreatedBy?.Name ?? string.Empty,
                DestinationUserName = entity.DestinationUser?.Name ?? string.Empty,
                CreatedAt = entity.CreatedAt,
                OriginSectorAcronym = entity.OriginSector?.Acronym ?? string.Empty,
                DestinationSectorAcronym = entity.DestinationSector?.Acronym ?? string.Empty
            };

            return CreatedAtRoute(nameof(GeProtocolByIdAsync), new { id = entity.Id }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a protocol by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the protocol.</param>
    [HttpGet("{id:guid}", Name = nameof(GeProtocolByIdAsync))]
    [ProducesResponseType(typeof(Protocol), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeProtocolByIdAsync(Guid id)
    {
        Protocol? protocol = 
            await _protocolService.GetByIdAsync(id);

        if (protocol == null)
            return NotFound();

        return Ok(protocol);
    }

    /// <summary>
    /// Retrieves all protocols records.
    /// </summary>
    /// Returns an <see cref="IActionResult"/> containing a list of protocols, wrapped in an HTTP 200 OK response.
    [HttpGet("show")]
    [ProducesResponseType(typeof(IEnumerable<Protocol>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        ICollection<Protocol> sectors = 
            await _protocolService.GetAllAsync();

        return Ok(sectors);
    }

    /// <summary>
    /// Gets a paginated result of protocols from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter protocols.</param>
    /// <returns>A paged result DTO containing the protocols and total count.</returns>
    [HttpGet("show_paged")]
    public async Task<IActionResult> GetPagedAsync(
    [FromQuery] int pageNumber = 1, 
    [FromQuery] int pageSize = 15, 
    [FromQuery] string? sortLabel = null, 
    [FromQuery] string? sortDirection = null, 
    [FromQuery] string? searchString = null)
    {
        ProtocolPagedResultDTO result = 
            await _protocolService.GetPagedAsync(
                pageNumber, 
                pageSize, 
                sortLabel, 
                sortDirection, 
                searchString
            );

        return Ok(result);
    }

    /// <summary>
    /// Retrieves the total number of protocols that match the given search criteria.
    /// </summary>
    /// <param name="searchString">A keyword used to filter protocols by name or other relevant fields (optional).</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing the total count of protocols as an integer, wrapped in an HTTP 200 OK response.
    /// </returns>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalCountAsync([FromQuery] string? searchString = null)
    {
        int total = 
            await _protocolService.GetTotalProtocolsCountAsync(searchString);

        return Ok(total);
    }

    /// <summary>
    /// Updates an existing protocol by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the protocol to update.</param>
    /// <param name="protocolDTO">The data transfer object with updated protocol information.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the updated <see cref="Protocol"/> if successful,
    /// or <see cref="NotFoundResult"/> if the protocol does not exist.
    /// </returns>
    [HttpPut("update_protocol/{id}")]
    [ProducesResponseType(typeof(Protocol), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] ProtocolUpdateDTO protocolDTO)
    {
        Protocol? entity = 
            await _protocolService.UpdateAsync(id, protocolDTO);

        if (entity == null)
            return NotFound();

        ProtocolResponseDTO response = new()
        {
            Id = entity.Id,
            Number = entity.Number,
            Subject = entity.Subject,
            Description = entity.Description,
            Status = entity.Status,
            IsArchived = entity.IsArchived,
            CreatedByName = entity.CreatedBy?.Name ?? string.Empty,
            DestinationUserName = entity.DestinationUser?.Name ?? string.Empty,
            CreatedAt = entity.CreatedAt,
            OriginSectorAcronym = entity.OriginSector?.Acronym ?? string.Empty,
            DestinationSectorAcronym = entity.DestinationSector?.Acronym ?? string.Empty,
            UpdatedAt = entity.UpdatedAt,
            UpdatedByName = entity.UpdatedBy?.Name ?? string.Empty
        };

        return Ok(response);
    }

    /// <summary>
    /// Deletes a protocol by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the protocol to delete.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with a success message if the protocol was deleted,
    /// <see cref="NotFoundResult"/> if the protocol does not exist,
    /// or <see cref="ConflictObjectResult"/> with an error message if the deletion is not allowed due to business rules.
    /// </returns>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            bool deleted = 
                await _protocolService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { Message = "Protocolo deletado com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }

    }
}