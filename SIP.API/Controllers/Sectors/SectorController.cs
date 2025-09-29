using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.DTOs.Sectors.Default;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;
using SIP.API.Domain.Interfaces.Sectors;

namespace SIP.API.Controllers.Sectors;

/// <summary>
/// API controller for managing "sector" entities.
/// </summary>
[Route("sip_api/[controller]")]
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
    [HttpPost("register_sector")]
    [ProducesResponseType(typeof(Sector), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] SectorCreateDTO sectorDTO)
    {
        _logger.LogInformation("Solicitação recebida para registrar um novo Setor. Payload: {@sectorDTO}", sectorDTO);

        try
        {
            Sector entity = await
                _sectorService.CreateAsync(sectorDTO);

            SectorResponseDTO response = new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Acronym = entity.Acronym,
                Phone = entity.Phone,
                CreatedAt = entity.CreatedAt
            };

            _logger.LogInformation("Setor '{Name}' registrado com sucesso com ID {Id} em {CreatedAt}.",
                entity.Name, entity.Id, entity.CreatedAt);

            return CreatedAtRoute(nameof(GetSectorByIdAsync), new { id = entity.Id }, response);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(
                argEx, "Dados inválidos fornecidos ao registrar o setor. Payload: {@sectorDTO}",
                sectorDTO);

            return BadRequest(new ErrorResponse("Invalid data: " + argEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Ocorreu um erro inesperado ao registrar o setor. Payload: {@sectorDTO}",
                sectorDTO);

            return BadRequest(new ErrorResponse("Ocorreu um erro inesperado."));
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
    [HttpGet("{id}", Name = "GetSectorByIdAsync")]
    [ProducesResponseType(typeof(Sector), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSectorByIdAsync(Guid id)
    {
        _logger.LogInformation("Solicitação recebida para buscar um Setor pelo ID. Payload: {@id}", id);

        try
        {
            Sector? sector =
                await _sectorService.GetByIdAsync(id);

            if (sector == null)
            {
                _logger.LogWarning(
                    "Nenhum Setor encontrado para o ID {SectorId}", id);

                return NotFound(new ErrorResponse($"Nenhum Setor encontrado para o ID {id}"));
            }

            SectorResponseDTO response = new()
            {
                Id = sector.Id,
                Name = sector.Name,
                Acronym = sector.Acronym,
                Phone = sector.Phone,
                CreatedAt = sector.CreatedAt,
                UpdatedAt = sector.UpdatedAt
            };

            _logger.LogInformation(
                "Setor encontrado. ID: {Id}, Nome: {Name}, Sigla: {Acronym}, CriadoEm: {CreatedAt}, AtualizadoEm: {UpdatedAt}",
                sector.Id, sector.Name, sector.Acronym, sector.CreatedAt, sector.UpdatedAt);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro inesperado ao buscar Setor pelo ID {SectorId}", id);

            return NotFound(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Retrieves all sectors records.
    /// </summary>
    /// Returns an <see cref="IActionResult"/> containing a list of sectors, wrapped in an HTTP 200 OK response.
    [HttpGet("show")]
    [ProducesResponseType(typeof(IEnumerable<Sector>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        _logger.LogInformation("Solicitação recebida para listar todos os setores.");

        try
        {
            ICollection<SectorDefaultDTO> sectors = await _sectorService.GetAllSectorsAsync();

            if (sectors == null || sectors.Count == 0)
            {
                _logger.LogWarning("Nenhum setor encontrado na base de dados.");
                return Ok(Enumerable.Empty<SectorDefaultDTO>());
            }

            _logger.LogInformation("Consulta concluída com sucesso. Total de setores encontrados: {Total}", sectors.Count);

            return Ok(sectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os setores.");
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
    [HttpGet("show_paged")]
    public async Task<IActionResult> GetPagedAsync(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 15,
    [FromQuery] string? sortLabel = null,
    [FromQuery] string? sortDirection = null,
    [FromQuery] string? searchString = null)
    {
        _logger.LogInformation(
            "Solicitação recebida para buscar setores paginados. PageNumber: {PageNumber}, PageSize: {PageSize}, SortLabel: {SortLabel}, SortDirection: {SortDirection}, SearchString: {@SearchString}",
            pageNumber, pageSize, sortLabel, sortDirection, searchString);

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
                    "Nenhum setor encontrado para os parâmetros fornecidos. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchString: {@SearchString}",
                    pageNumber, pageSize, searchString);
            }
            else
            {
                _logger.LogInformation(
                    "Consulta de setores paginados concluída com sucesso. Total de registros retornados: {ReturnedCount}, Total geral: {TotalCount}",
                    result.Items.Count, result.TotalCount);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Erro inesperado ao buscar setores paginados. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchString: {@SearchString}",
                pageNumber, pageSize, searchString);

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
    public async Task<ActionResult<int>> GetTotalCountAsync([FromQuery] string? searchString = null)
    {
        _logger.LogInformation(
            "Solicitação recebida para contar setores. SearchString: {@SearchString}",
            searchString);

        try
        {
            int total = await _sectorService.GetTotalSectorsCountAsync(searchString);

            _logger.LogInformation(
                "Total de setores encontrados: {Total}. SearchString: {@SearchString}",
                total, searchString);

            return Ok(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Erro inesperado ao contar setores. SearchString: {@SearchString}",
                searchString);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocorreu um erro inesperado ao contar os setores."));
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
    [HttpPut("update_sector/{id}")]
    [ProducesResponseType(typeof(SectorResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
    Guid id,
    [FromBody] SectorUpdateDTO sectorDTO)
    {
        _logger.LogInformation(
            "Solicitação recebida para atualizar setor. ID: {SectorId}, Payload: {@sectorDTO}",
            id, sectorDTO);

        try
        {
            Sector? updated = await _sectorService.UpdateAsync(id, sectorDTO);

            if (updated == null)
            {
                _logger.LogWarning("Nenhum setor encontrado para atualização. ID: {sectorDTO}", id);
                return NotFound(new ErrorResponse($"Nenhum setor encontrado para o ID {id}"));
            }

            SectorResponseDTO response = new()
            {
                Id = updated.Id,
                Name = updated.Name,
                Acronym = updated.Acronym,
                Phone = updated.Phone,
                CreatedAt = updated.CreatedAt,
                UpdatedAt = updated.UpdatedAt
            };

            _logger.LogInformation(
                "Setor atualizado com sucesso. ID: {SectorId}, Nome: {Name}, Sigla: {Acronym}",
                updated.Id, updated.Name, updated.Acronym);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Erro inesperado ao atualizar setor. ID: {SectorId}, Payload: {@SectorDTO}",
                id, sectorDTO);

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
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Solicitação recebida para deletar setor. ID: {SectorId}", id);

        try
        {
            bool deleted = await _sectorService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Nenhum setor encontrado para exclusão. ID: {SectorId}", id);
                return NotFound(new ErrorResponse($"Nenhum setor encontrado para o ID {id}"));
            }

            _logger.LogInformation("Setor deletado com sucesso. ID: {SectorId}", id);
            return Ok(new { Message = "Setor deletado com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex, "Falha ao deletar setor devido a restrições de negócio. ID: {SectorId}", id);

            return Conflict(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Erro inesperado ao deletar setor. ID: {SectorId}", id);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocorreu um erro inesperado ao deletar o setor."));
        }
    }
}