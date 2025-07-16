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
            return BadRequest(new ErrorResponse(ex.Message));
            
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
    /// Retrieves all sectors records.
    /// </summary>
    /// Returns an <see cref="IActionResult"/> containing a list of sectors, wrapped in an HTTP 200 OK response.
    [HttpGet("show")]
    [ProducesResponseType(typeof(IEnumerable<Sector>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSectorsAsync()
    {
        var sectors = await _sectorService.GetAllSectorsAsync();
        return Ok(sectors);
    }

    /// <summary>
    /// Gets a paginated result of sectors from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A paged result DTO containing the sectors and total count.</returns>
    [HttpGet("show_paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 15, [FromQuery] string? sortLabel = null, [FromQuery] string? sortDirection = null, [FromQuery] string? searchString = null)
    {
        var result = await _sectorService.GetPagedAsync(pageNumber, pageSize, sortLabel, sortDirection, searchString);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the total number of sectors that match the given search criteria.
    /// </summary>
    /// <param name="searchString">A keyword used to filter sectors by name or other relevant fields (optional).</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing the total count of sectors as an integer, wrapped in an HTTP 200 OK response.
    /// </returns>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalSectorsCountAsync([FromQuery] string? searchString = null)
    {
        var total = await _sectorService.GetTotalSectorsCountAsync(searchString);
        return Ok(total);
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

            return Ok(new { Message = "Secretaria deletada com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse(ex.Message));
        }
    }
}