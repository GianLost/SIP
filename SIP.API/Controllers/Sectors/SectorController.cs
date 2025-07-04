using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Interfaces.Sectors;

namespace SIP.API.Controllers.Sectors;

/// <summary>
/// API controller for managing "sector" entities.
/// </summary>
[Route("sip_api/[controller]")]
[ApiController]
public class SectorController(ISector sector) : ControllerBase
{
    private readonly ISector _sectorService = sector;

    /// <summary>
    /// Registers a new sector in the system.
    /// </summary>
    /// <param name="sectorDTO">The data transfer object containing the sector's information.</param>
    /// <returns>
    /// Returns <see cref="CreatedAtActionResult"/> with the created sector and location header if successful,
    /// or <see cref="BadRequestObjectResult"/> with error details if the request is invalid.
    /// </returns>
    [HttpPost("register_sector")]
    [ProducesResponseType(typeof(Sector), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] SectorCreateDTO sectorDTO)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            Sector entity = await _sectorService.CreateAsync(sectorDTO);

            var response = new SectorResponseDTO
            {
                Name = entity.Name,
                Acronym = entity.Acronym,
                Phone = entity.Phone,
                CreatedAt = entity.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a sector by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sector.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the <see cref="Sector"/> if found,
    /// or <see cref="NotFoundResult"/> if no sector exists with the specified ID.
    /// </returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Sector), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        Sector? sector = await _sectorService.GetByIdAsync(id);

        if (sector == null)
            return NotFound();

        return Ok(sector);
    }

    /// <summary>
    /// Retrieves a paginated list of sectors.
    /// </summary>
    /// <param name="pageNumber">The page number (default is 1).</param>
    /// <param name="pageSize">The number of records per page (default is 20).</param>
    /// <returns>A paginated list of sectors.</returns>
    [HttpGet("show")]
    [ProducesResponseType(typeof(IEnumerable<Sector>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var sectors = await _sectorService.GetAllAsync(pageNumber, pageSize);
        return Ok(sectors);
    }

    /// <summary>
    /// Updates an existing sector by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the sector to update.</param>
    /// <param name="sectorDTO">The data transfer object with updated sector information.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the updated <see cref="Sector"/> if successful,
    /// or <see cref="NotFoundResult"/> if the sector does not exist.
    /// </returns>
    [HttpPut("update_sector/{id}")]
    [ProducesResponseType(typeof(Sector), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] SectorUpdateDTO sectorDTO)
    {
        Sector? updated = await _sectorService.UpdateAsync(id, sectorDTO);

        if (updated == null)
            return NotFound();

        var response = new SectorResponseDTO
        {
            Name = updated.Name,
            Acronym = updated.Acronym,
            Phone = updated.Phone,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated?.UpdatedAt ?? null
        };

        return Ok(response);
    }

    /// <summary>
    /// Deletes a sector by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the sector to delete.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with a success message if the sector was deleted,
    /// <see cref="ConflictObjectResult"/> with a friendly message if there are linked users,
    /// or <see cref="NotFoundResult"/> if the sector does not exist.
    /// </returns>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var deleted = await _sectorService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { Message = "Sector was deleted with success." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
    }
}