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
using SIP.API.Domain.Helpers.Extensions;
using SIP.API.Controllers.Errors;

namespace SIP.API.Controllers.Sectors;

/// <summary>
/// Controller responsável por gerenciar operações relacionadas aos <c>Setores</c>.
/// </summary>
/// <remarks>
/// Um <c>Setor</c> representa uma área ou departamento dentro da organização.
/// Esta controller fornece endpoints para criação, consulta, atualização e exclusão de setores.
/// </remarks>
[Route("sip_api/sectors")]
[ApiController]
public class SectorController(ISector sector, ILogger<SectorController> logger) : ControllerBase
{
    private readonly ISector _sectorService = sector;
    private readonly ILogger<SectorController> _logger = logger;

    /// <summary>
    /// Cria um novo setor no sistema.
    /// </summary>
    /// <param name="sectorDTO">Objeto contendo os dados necessários para criar um setor.</param>
    /// <returns>
    /// Retorna <see cref="CreatedAtActionResult"/> com o setor criado e o cabeçalho <c>Location</c>,
    /// ou <see cref="BadRequestObjectResult"/> caso os dados fornecidos sejam inválidos.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando os dados fornecidos são inválidos (ex: nome duplicado, campos obrigatórios ausentes).</exception>
    /// <exception cref="Exception">Erro inesperado durante o processo de criação.</exception>
    /// <remarks>
    /// Ação: <b>Criar setor</b>.  
    /// - Retorna 201 (Created) em caso de sucesso.  
    /// - Retorna 400 (Bad Request) quando os dados não são válidos.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(SectorResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SectorResponseDTO>> CreateAsync([FromBody] SectorCreateDTO sectorDTO)
    {
        _logger.LogInformation<Sector>(
            message: LogInfoMessages.CreateRequest, 
            args: sectorDTO);

        try
        {
            Sector entity = await
                _sectorService.CreateAsync(sectorDTO);

            _logger.LogInformation<Sector>(
                message: LogSuccessMessages.Created, 
                args: 
                [
                    entity.Id, 
                    entity.Name, 
                    entity.CreatedAt
                ]);

            return CreatedAtRoute(
                routeName: "GetSectorByIdAsync", 
                routeValues: new { id = entity.Id }, 
                value: ToResponse(entity));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<Sector>(
                exception: ex,
                message: LogWarningMessages.InvalidCreate, 
                args: sectorDTO);

            return BadRequest(
                error: new ErrorResponse("Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<Sector>(
                exception: ex,
                message: LogErrorMessages.CreateError, 
                args: sectorDTO);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError, 
                value: new ErrorResponse("Ocorreu um erro inesperado."));
        }
    }

    /// <summary>
    /// Obtém um setor pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único (GUID) do setor.</param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com os dados do setor, se encontrado.  
    /// - <see cref="NotFoundObjectResult"/> caso o setor não exista.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao consultar o setor.</exception>
    /// <remarks>
    /// Ação: <b>Consultar setor por ID</b>.  
    /// - Retorna 200 (OK) com os dados do setor.  
    /// - Retorna 404 (Not Found) se o setor não existir.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet("{id}", Name = "GetSectorByIdAsync")]
    [ProducesResponseType(typeof(SectorResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SectorResponseDTO>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation<Sector>(
            message: LogInfoMessages.GetByIdRequest,
            args: id);

        try
        {
            Sector? sector =
                await _sectorService.GetByIdAsync(id);

            if (sector == null)
            {
                _logger.LogWarning<Sector>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum Setor encontrado para o ID {id}"));
            }

            _logger.LogInformation<Sector>(
                message: LogSuccessMessages.FoundById,
                args: id);

            return Ok(ToResponse(sector));
        }
        catch (Exception ex)
        {
            _logger.LogError<Sector>(
                exception: ex,
                message: LogErrorMessages.GetByIdError,
                args: id);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse("Ocorreu um erro inesperado ao consultar o setor pelo ID.")
            );
        }
    }

    /// <summary>
    /// Obtém todos os setores cadastrados.
    /// </summary>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com a lista de setores.
    /// Se não houver registros, retorna uma lista vazia.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao buscar todos os setores.</exception>
    /// <remarks>
    /// Ação: <b>Listar todos os setores</b>.  
    /// - Retorna 200 (OK) com a lista de setores.  
    /// - Retorna lista vazia se não houver setores cadastrados.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SectorDefaultDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SectorDefaultDTO>>> GetAllAsync()
    {
        _logger.LogInformation<Sector>(
            message: LogInfoMessages.GetAllRequest);

        try
        {
            ICollection<SectorDefaultDTO> sectors = 
                await _sectorService.GetAllSectorsAsync();

            if (sectors == null || sectors.Count == 0)
            {
                _logger.LogWarning<Sector>(
                    message: LogWarningMessages.Empty);

                return Ok(Enumerable.Empty<SectorDefaultDTO>());
            }

            _logger.LogInformation<Sector>(
                message: LogSuccessMessages.FoundAll,
                args: sectors.Count);

            return Ok(sectors);
        }
        catch (Exception ex)
        {
            _logger.LogError<Sector>(
                exception: ex,
                message: LogErrorMessages.GetAllError);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError, 
                value: new ErrorResponse("Ocorreu um erro inesperado ao buscar todos os setores."));
        }
    }

    /// <summary>
    /// Obtém setores de forma paginada.
    /// </summary>
    /// <param name="pageNumber">Número da página (inicia em 1).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <param name="sortLabel">Campo para ordenação.</param>
    /// <param name="sortDirection">Direção da ordenação ("asc" ou "desc").</param>
    /// <param name="searchString">Filtro opcional para busca por nome ou outros campos.</param>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com a lista paginada e o total de registros encontrados.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao realizar a paginação.</exception>
    /// <remarks>
    /// Ação: <b>Listar setores paginados</b>. 
    /// 
    /// <b>Endpoint para carregar dados em tabelas e grades de forma eficiente.</b>
    /// - Retorna 200 (OK) com a lista de setores e total de registros.  
    /// - Retorna lista vazia se nenhum registro for encontrado para os filtros aplicados.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
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
        _logger.LogInformation<Sector>(
            message: LogInfoMessages.PaginationRequest,
            args: 
            [
                pageNumber, 
                pageSize, 
                sortLabel, 
                sortDirection, 
                searchString
            ]);

        try
        {
            SectorPagedResultDTO result =
                await _sectorService.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    sortLabel,
                    sortDirection,
                    searchString
                );

            if (result.Items == null || result.Items.Count == 0)
            {
                _logger.LogWarning<Sector>(
                    message: LogWarningMessages.EmptyPagination,
                    args: 
                    [
                        pageNumber, 
                        pageSize, 
                        searchString
                    ]);
            }
            else
            {
                _logger.LogInformation<Sector>(
                    message: LogSuccessMessages.FoundPaged,
                    args: 
                    [
                        result.Items.Count, 
                        result.TotalCount
                    ]);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError<Sector>(
                exception: ex, 
                message: LogErrorMessages.PaginationError,
                args: 
                [
                    pageNumber, 
                    pageSize, 
                    searchString
                ]);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: new ErrorResponse("Ocorreu um erro inesperado ao buscar setores paginados."));
        }
    }

    /// <summary>
    /// Obtém a quantidade total de setores cadastrados.
    /// </summary>
    /// <param name="searchString">Filtro opcional para restringir a contagem.</param>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com o total de setores encontrados.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao contar os setores.</exception>
    /// <remarks>
    /// Ação: <b>Contar setores</b>.  
    /// - Retorna 200 (OK) com a contagem.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetTotalCountAsync([FromQuery] string? searchString = null)
    {
        _logger.LogInformation<Sector>(
            message: LogInfoMessages.CountRequest,
            args: searchString);

        try
        {
            int total = 
                await _sectorService.GetTotalSectorsCountAsync(searchString);

            _logger.LogInformation<Sector>(
                message: LogSuccessMessages.Counted,
                args: 
                [
                    total, 
                    searchString
                ]);

            return Ok(total);
        }
        catch (Exception ex)
        {
            _logger.LogError<Sector>(
                exception: ex, 
                message: LogErrorMessages.CountError,
                args: searchString);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError, 
                value: new ErrorResponse("Ocorreu um erro inesperado ao contar os setores."));
        }
    }

    /// <summary>
    /// Atualiza os dados de um setor existente.
    /// </summary>
    /// <param name="id">Identificador único do setor.</param>
    /// <param name="sectorDTO">Objeto contendo os novos dados do setor.</param>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com o setor atualizado,
    /// <see cref="NotFoundObjectResult"/> caso o setor não exista
    /// ou <see cref="BadRequestObjectResult"/> caso os dados fornecidos sejam inválidos.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao atualizar o setor.</exception>
    /// <remarks>
    /// Ação: <b>Atualizar setor</b>.  
    /// - Retorna 200 (OK) em caso de sucesso.  
    /// - Retorna 404 (Not Found) se o setor não for encontrado.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SectorResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SectorResponseDTO>> UpdateAsync(Guid id, [FromBody] SectorUpdateDTO sectorDTO)
    {
        _logger.LogInformation<Sector>(
            message: LogInfoMessages.UpdateRequest,
            args: 
            [
                id, 
                sectorDTO
            ]);

        try
        {
            Sector? updated = 
                await _sectorService.UpdateAsync(id, sectorDTO);

            if (updated == null)
            {
                _logger.LogWarning<Sector>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum setor encontrado para o ID {id}"));
            }

            _logger.LogInformation<Sector>(
                message: LogSuccessMessages.Updated,
                args: id);

            return Ok(ToResponse(updated));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<Sector>(
                exception: ex,
                message: LogWarningMessages.InvalidUpdate,
                args:
                [
                    id,
                    sectorDTO
                ]);

            return BadRequest(
                error: new ErrorResponse("Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<Sector>(
                exception: ex, 
                message: LogErrorMessages.UpdateError,
                args: 
                [
                    id, 
                    sectorDTO
                ]);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: new ErrorResponse("Ocorreu um erro inesperado ao atualizar o setor."));
        }
    }

    /// <summary>
    /// Exclui um setor pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do setor.</param>
    /// <returns>
    /// Retorna <see cref="NoContentResult"/> se o setor for excluído com sucesso,  
    /// <see cref="NotFoundObjectResult"/> se o setor não existir,  
    /// ou <see cref="ConflictObjectResult"/> se houver entidades vinculadas (ex: usuários).  
    /// </returns>
    /// <exception cref="InvalidOperationException">Setor não pode ser excluído devido a vínculos.</exception>
    /// <exception cref="Exception">Erro inesperado ao excluir o setor.</exception>
    /// <remarks>
    /// Ação: <b>Excluir setor</b>.  
    /// 
    ///  **Regras de Negócio:** Um setor não pode ser excluído se houver entidades associadas a ele (ex: usuários, protocolos).
    ///   Neste caso, a API retornará um status 409 (Conflict) com uma mensagem explicativa.
    /// - Retorna 204 (No Content) em caso de exclusão bem-sucedida.  
    /// - Retorna 404 (Not Found) se o setor não for encontrado.  
    /// - Retorna 409 (Conflict) se houver vínculos impeditivos para exclusão.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogInformation<Sector>(
            message: LogInfoMessages.DeleteRequest,
            args: id);

        try
        {
            bool deleted = 
                await _sectorService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning<Sector>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum setor encontrado para o ID {id}"));
            }

            _logger.LogInformation<Sector>(
                message: LogSuccessMessages.Deleted,
                args: id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning<Sector>(
                exception: ex, 
                message: LogWarningMessages.InvalidOperation,
                args: id);

            return Conflict(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<Sector>(
                exception: ex, 
                message: LogErrorMessages.DeleteError,
                args: id);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError, 
                value: new ErrorResponse("Ocorreu um erro inesperado ao deletar o setor."));
        }
    }

    /// <summary>
    /// Converte uma entidade <see cref="Sector"/> em um objeto de resposta padronizado <see cref="SectorResponseDTO"/>.
    /// </summary>
    /// <param name="entity">Entidade <see cref="Sector"/> obtida da camada de domínio.</param>
    /// <returns>
    /// Retorna um objeto <see cref="SectorResponseDTO"/> contendo os dados essenciais do setor
    /// que serão expostos pela API.
    /// </returns>
    /// <remarks>
    /// Este método garante que apenas informações relevantes e seguras sejam retornadas aos consumidores da API,
    /// servindo como camada de mapeamento entre a entidade de domínio e o contrato de saída.
    /// 
    /// **Campos retornados:**
    /// - <c>Id</c> → Identificador único do setor.  
    /// - <c>Name</c> → Nome do setor.  
    /// - <c>Acronym</c> → Sigla ou abreviação do setor.  
    /// - <c>Phone</c> → Telefone de contato do setor.  
    /// - <c>CreatedAt</c> → Data de criação do registro.  
    /// - <c>UpdatedAt</c> → Data da última atualização do registro (se houver).  
    /// </remarks>
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