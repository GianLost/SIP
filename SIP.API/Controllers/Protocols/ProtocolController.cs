using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.Interfaces.Protocols;
using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.DTOs.Protocols.Responses;

namespace SIP.API.Controllers.Protocols;

[Route("api/[controller]")]
[ApiController]
public class ProtocolController(IProtocol protocol) : ControllerBase
{
    private readonly IProtocol _protocolService = protocol;

    /// <summary>
    /// Registers a new protocol in the system.
    /// </summary>
    /// <param name="protocolDTO">The data transfer object containing the protocol's information.</param>
    /// <returns>
    /// Returns <see cref="CreatedAtActionResult"/> with the created protocol and location header if successful,
    /// or <see cref="BadRequestObjectResult"/> with error details if the request is invalid.
    /// </returns>
    [HttpPost("register_protocol")]
    [ProducesResponseType(typeof(Protocol), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] ProtocolCreateDTO protocolDTO)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            Protocol entity = await _protocolService.CreateAsync(protocolDTO);

            ProtocolResponseDTO response = new()
            {
                Number = entity.Number,
                Subject = entity.Subject,
                IsArchived = entity.IsArchived,
                CreatedAt = entity.CreatedAt
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
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the <see cref="Protocol"/> if found,
    /// or <see cref="NotFoundResult"/> if no protocol exists with the specified ID.
    /// </returns>
    [HttpGet("{id}", Name = "GeProtocolByIdAsync")]
    [ProducesResponseType(typeof(Protocol), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeProtocolByIdAsync(Guid id)
    {
        Protocol? protocol = await _protocolService.GetByIdAsync(id);
        if (protocol == null)
            return NotFound();
        return Ok(protocol);
    }
}