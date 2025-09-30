using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Interfaces.Sectors;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Default;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;
using SIP.API.Domain.Helpers.Messages.LogMessage.Info;
using SIP.API.Domain.Helpers.Messages.LogMessage.Success;
using SIP.API.Domain.Helpers.Messages.LogMessage.Warning;
using SIP.API.Domain.Helpers.Messages.LogMessage.Error;
using SIP.API.Controllers.Errors;

namespace SIP.API.Controllers.Sectors;

/// <summary>
/// API controller for managing "sector" entities.
/// </summary>
[Route("sip_api/sectors")]
[ApiController]
public class SectorController(ISector sector, ILogger<SectorController> logguer) : ControllerBase
{
    private readonly ISector _sectorService = sector;
    private readonly ILogger<SectorController> _logger = logguer;

    /// <summary>
    /// Registers a new sector in the system.
    /// </summary>
    /// <param name="sectorDTO">The data transfer object containing the sector's information.</param>
    /// <returns>
    /// Returns <see cref="CreatedAtActionResult"/> with the created sector and location header if successful,
    /// or <see cref="BadRequestObjectResult"/> with error details if the request is invalid.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(SectorResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SectorResponseDTO>> CreateAsync([FromBody] SectorCreateDTO sectorDTO)
    {
        _logger.LogInformation(
            message: LogInfoMessages.CreateRequest, 
            "Sector", 
            sectorDTO);

        try
        {
            Sector entity = await
                _sectorService.CreateAsync(sectorDTO);

            _logger.LogInformation(
                message: LogSuccessMessages.Created,
                "Sector",
                entity.Id,
                entity.Name, 
                entity.CreatedAt); // log success

            return CreatedAtRoute(
                routeName: nameof(GetByIdAsync), 
                routeValues: new { id = entity.Id }, 
                value: ToResponse(entity));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                exception: ex, 
                message: LogWarningMessages.InvalidCreate,
                "Sector",
                sectorDTO);

            return BadRequest(new ErrorResponse("Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                exception: ex, 
                message: LogErrorMessages.CreateError,
                "Sector",
                sectorDTO);

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ocorreu um erro inesperado."));
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
    [HttpGet("{id}", Name = "GetByIdAsync")]
    [ProducesResponseType(typeof(SectorResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SectorResponseDTO>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation(
            message: LogInfoMessages.GetByIdRequest,
            "Sector",
            id);

        try
        {
            Sector? sector =
                await _sectorService.GetByIdAsync(id);

            if (sector == null)
            {
                _logger.LogWarning(
                    message: LogWarningMessages.NotFound,
                    "Sector",
                    id);

                return NotFound(new ErrorResponse($"Nenhum Setor encontrado para o ID {id}"));
            }

            _logger.LogInformation(
                message: LogSuccessMessages.FoundById,
                "Sector",
                id);

            return Ok(ToResponse(sector));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                exception: ex, 
                message: LogErrorMessages.GetByIdError,
                "Sector",
                id);

            return NotFound(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Retrieves all sectors records.
    /// </summary>
    /// Returns an <see cref="IActionResult"/> containing a list of sectors, wrapped in an HTTP 200 OK response.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SectorDefaultDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SectorDefaultDTO>>> GetAllAsync()
    {
        _logger.LogInformation(
            message: LogInfoMessages.GetAllRequest,
            "Sectors");

        try
        {
            ICollection<SectorDefaultDTO> sectors = 
                await _sectorService.GetAllSectorsAsync();

            if (sectors == null || sectors.Count == 0)
            {
                _logger.LogWarning(
                    message: LogWarningMessages.Empty,
                    "Sectors");

                return Ok(Enumerable.Empty<SectorDefaultDTO>());
            }

            _logger.LogInformation(
                message: LogSuccessMessages.FoundAll,
                "Sectors",
                sectors.Count);

            return Ok(sectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                exception: ex, 
                message: LogErrorMessages.GetAllError,
                "Sectors");

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse(ex.Message));
        }
    }

    /*
     Refatored GetAllAsync to future use.
     */
    ///// <summary>
    ///// Retrieves all sectors records.
    ///// </summary>
    ///// Returns an <see cref="IActionResult"/> containing a list of sectors, wrapped in an HTTP 200 OK response.
    //[HttpGet("show")]
    //[ProducesResponseType(typeof(IEnumerable<Sector>), StatusCodes.Status200OK)]
    //public async Task<IActionResult> GetAllAsync()
    //{
    //    try
    //    {
    //        ICollection<Sector> sectors =
    //            await _sectorService.GetAllSectorsAsync();

    //        if (sectors == null || sectors.Count == 0)
    //        {
    //            return Ok(new SectorPagedResultDTO
    //            {
    //                Items = [],
    //                TotalCount = 0
    //            });
    //        }

    //        SectorPagedResultDTO result = new()
    //        {
    //            Items = [.. sectors.Select(s => new SectorListItemDTO
    //            {
    //                Id = s.Id,
    //                Name = s.Name,
    //                Acronym = s.Acronym,
    //                Phone = s.Phone,
    //                CreatedAt = s.CreatedAt,
    //                CreatedById = s.CreatedById,
    //                CreatedBy = s.CreatedBy,
    //                UpdatedAt = s.UpdatedAt,
    //                UpdatedById = s.UpdatedById,
    //                UpdatedBy = s.UpdatedBy,
    //                Users = s.Users
    //            })],
    //            TotalCount = sectors.Count
    //        };

    //        return Ok(result);
    //    }
    //    catch (Exception)
    //    {
    //        return StatusCode(StatusCodes.Status500InternalServerError,
    //            new ErrorResponse("Ocorreu um erro interno ao buscar os setores."));
    //    }
    //}
   /// <summary>
    /// Gets a paginated result of sectors from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A paged result DTO containing the sectors and total count.</returns>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(SectorPagedResultDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SectorPagedResultDTO>> GetPagedAsync(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 15,
    [FromQuery] string? sortLabel = null,
    [FromQuery] string? sortDirection = null,
    [FromQuery] string? searchString = null)
    {
        _logger.LogInformation(
            message: LogInfoMessages.PaginationRequest,
            "Sectors",
            pageNumber, 
            pageSize, 
            sortLabel, 
            sortDirection, 
            searchString);

        try
        {
            SectorPagedResultDTO result =
                await _sectorService.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    sortLabel,
                    sortDirection,
                    searchString);

            if (result.Items == null || result.Items.Count == 0)
            {
                _logger.LogWarning(
                    message: LogWarningMessages.EmptyPagination,
                    "Sectors",
                    pageNumber, 
                    pageSize, 
                    searchString);
            }
            else
            {
                _logger.LogInformation(
                    message: LogSuccessMessages.FoundPaged,
                    "Sectors",
                    result.Items.Count, 
                    result.TotalCount);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                exception: ex, 
                message: LogErrorMessages.PaginationError,
                "Sectors",
                pageNumber, 
                pageSize, 
                searchString);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocorreu um erro inesperado ao buscar setores paginados."));
        }
    }

    /// <summary>
    /// Retrieves the total number of sectors that match the given search criteria.
    /// </summary>
    /// <param name="searchString">A keyword used to filter sectors by name or other relevant fields (optional).</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing the total count of sectors as an integer, wrapped in an HTTP 200 OK response.
    /// </returns>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetTotalCountAsync([FromQuery] string? searchString = null)
    {
        _logger.LogInformation(
            message: LogInfoMessages.CountRequest,
            "Sectors",
            searchString);

        try
        {
            int total = 
                await _sectorService.GetTotalSectorsCountAsync(searchString);

            _logger.LogInformation(
                message: LogSuccessMessages.Counted,
                "Sectors",
                total, 
                searchString);

            return Ok(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                exception: ex, 
                message: LogErrorMessages.CountError,
               "Sectors",
                searchString);

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ocorreu um erro inesperado ao contar os setores."));
        }
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
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SectorResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SectorResponseDTO>> UpdateAsync(Guid id, [FromBody] SectorUpdateDTO sectorDTO)
    {
        _logger.LogInformation(
            message: LogInfoMessages.UpdateRequest,
            "Sector",
            id, 
            sectorDTO);

        try
        {
            Sector? updated = 
                await _sectorService.UpdateAsync(id, sectorDTO);

            if (updated == null)
            {
                _logger.LogWarning(
                    message: LogWarningMessages.NotFound,
                    "Sector",
                    id);

                return NotFound(new ErrorResponse($"Nenhum setor encontrado para o ID {id}"));
            }

            _logger.LogInformation(
                message: LogSuccessMessages.Updated,
                "Sector",
                id);

            return Ok(ToResponse(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                exception: ex, 
                message: LogErrorMessages.UpdateError,
                "Sector",
                id, 
                sectorDTO);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocorreu um erro inesperado ao atualizar o setor."));
        }
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
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogInformation(
            message: LogInfoMessages.DeleteRequest,
            "Sector",
            id);

        try
        {
            bool deleted = 
                await _sectorService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning(
                    message: LogWarningMessages.NotFound,
                    "Sector",
                    id);

                return NoContent();
            }

            _logger.LogInformation(
                message: LogSuccessMessages.Deleted,
                "Sector",
                id);

            return Ok(new { Message = "Setor deletado com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                exception: ex, 
                message: LogWarningMessages.InvalidOperation,
                "Sector",
                id);

            return Conflict(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                exception: ex, 
                message: LogErrorMessages.DeleteError,
                "Sector",
                id);

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ocorreu um erro inesperado ao deletar o setor."));
        }
    }

    private static SectorResponseDTO ToResponse(Sector entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Acronym = entity.Acronym,
        Phone = entity.Phone,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}