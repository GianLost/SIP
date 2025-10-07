using Microsoft.AspNetCore.Mvc;
using SIP.API.Controllers.Errors;
using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.DTOs.Protocols.Default;
using SIP.API.Domain.DTOs.Protocols.Pagination;
using SIP.API.Domain.DTOs.Protocols.Responses;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Helpers.Extensions;
using SIP.API.Domain.Helpers.Messages.LogMessages.Error;
using SIP.API.Domain.Helpers.Messages.LogMessages.Info;
using SIP.API.Domain.Helpers.Messages.LogMessages.Success;
using SIP.API.Domain.Helpers.Messages.LogMessages.Warning;
using SIP.API.Domain.Interfaces.Protocols;

namespace SIP.API.Controllers.Protocols;

/// <summary>
/// Controller responsável por gerenciar operações relacionadas aos <c>Protocolos</c>.
/// </summary>
/// <remarks>
/// Um <c>Protocolo</c> representa o objeto de um relatório que contém os dados de origem e destino, numeração única e sequencial, status e prioridade na transição do protocolo.
/// Esta controller fornece endpoints para criação, consulta, atualização, manutenção e exclusão de protocolos.
/// </remarks>
[Route("sip_api/protocols")]
[ApiController]
public class ProtocolController(IProtocol protocol, ILogger<ProtocolController> logger) : ControllerBase
{
    private readonly IProtocol _protocolService = protocol;
    private readonly ILogger<ProtocolController> _logger = logger;

    /// <summary>
    /// Cria um novo protocolo no sistema.
    /// </summary>
    /// <param name="protocolDTO">Objeto contendo os dados necessários para criar um protocolo.</param>
    /// <returns>
    /// Retorna <see cref="CreatedAtActionResult"/> com o protocolo criado e o cabeçalho <c>Location</c>,
    /// ou <see cref="BadRequestObjectResult"/> caso os dados fornecidos sejam inválidos.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando os dados fornecidos são inválidos (ex: número duplicado, campos obrigatórios ausentes).</exception>
    /// <exception cref="Exception">Erro inesperado durante o processo de criação.</exception>
    /// <remarks>
    /// Ação: <b>Gerar protocolo</b>.  
    /// - Retorna 201 (Created) em caso de sucesso.  
    /// - Retorna 400 (Bad Request) quando os dados não são válidos.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ProtocolResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProtocolResponseDTO>> CreateAsync([FromBody] ProtocolCreateDTO protocolDTO)
    {
        _logger.LogInformation<Protocol>(
            message: LogInfoMessages.CreateRequest,
            args: protocolDTO);

        try
        {
            Protocol entity =
                await _protocolService.CreateAsync(protocolDTO);

            _logger.LogInformation<Protocol>(
            message: LogSuccessMessages.Created,
            args:
            [
                entity.Id,
                entity.Status,
                entity.Number,
                entity.Subject,
                entity.CreatedBy?.Name,
                entity.DestinationUser?.Name,
                entity.OriginSector?.Acronym,
                entity.IsArchived,
                entity.CreatedAt
            ]);

            return CreatedAtRoute(
                routeName: "GetProtocolByIdAsync",
                routeValues: new { id = entity.Id },
                value: ToResponse(entity));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<Protocol>(
                exception: ex,
                message: LogWarningMessages.InvalidCreate,
                args: protocolDTO);

            return BadRequest(
                error: new ErrorResponse("Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<Protocol>(
                exception: ex,
                message: LogErrorMessages.CreateError,
                args: protocolDTO);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: new ErrorResponse("Ocorreu um erro inesperado."));
        }
    }

    /// <summary>
    /// Obtém um protocolo pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único (GUID) do protocolo.</param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com os dados do protocolo, se encontrado.  
    /// - <see cref="NotFoundObjectResult"/> caso o protocolo não exista.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao consultar o protocolo.</exception>
    /// <remarks>
    /// Ação: <b>Consultar protocolo por ID</b>.  
    /// - Retorna 200 (OK) com os dados do protocolo.  
    /// - Retorna 404 (Not Found) se o protocolo não existir.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet("{id}", Name = "GetProtocolByIdAsync")]
    [ProducesResponseType(typeof(ProtocolResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProtocolResponseDTO>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation<Protocol>(
            message: LogInfoMessages.GetByIdRequest,
            args: id);

        try
        {
            Protocol? protocol =
           await _protocolService.GetByIdAsync(id);

            if (protocol == null)
            {
                _logger.LogWarning<Protocol>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum protocolo encontrado para o ID {id}"));
            }

            return Ok(ToResponse(protocol));
        }
        catch (Exception ex)
        {
            _logger.LogError<Protocol>(
                exception: ex,
                message: LogErrorMessages.GetByIdError,
                args: id);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse("Ocorreu um erro inesperado ao consultar o protocolo pelo ID.")
            );
        }
    }

    /// <summary>
    /// Obtém todos os protocolos cadastrados.
    /// </summary>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com a lista de protocolos.
    /// Se não houver registros, retorna uma lista vazia.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao buscar todos os protocolos.</exception>
    /// <remarks>
    /// Ação: <b>Listar todos os protocolos</b>.  
    /// - Retorna 200 (OK) com a lista de protocolos.  
    /// - Retorna lista vazia se não houver protocolos cadastrados.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProtocolDefaultDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProtocolDefaultDTO>>> GetAllAsync()
    {
        _logger.LogInformation<Protocol>(
           message: LogInfoMessages.GetAllRequest);
        
        try
        {
            ICollection<Protocol> protocols =
                await _protocolService.GetAllAsync();

            if (protocols == null || protocols.Count == 0)
            {
                _logger.LogWarning<Protocol>(
                    message: LogWarningMessages.Empty);

                return Ok(Enumerable.Empty<ProtocolDefaultDTO>());
            }

            _logger.LogInformation<Protocol>(
                message: LogSuccessMessages.FoundAll,
                args: protocols.Count);

            return Ok(protocols);
        }
        catch (Exception ex)
        {
            _logger.LogError<Protocol>(
                exception: ex,
                message: LogErrorMessages.GetAllError);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse("Ocorreu um erro inesperado ao buscar todos os protocolos.")
            );
        }
    }

    /// <summary>
    /// Obtém protocolos de forma paginada.
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
    /// Ação: <b>Listar usuários paginados</b>. 
    /// 
    /// <b>Endpoint para carregar dados em tabelas e grades de forma eficiente.</b>
    /// - Retorna 200 (OK) com a lista de usuários e total de registros.  
    /// - Retorna lista vazia se nenhum registro for encontrado para os filtros aplicados.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(ProtocolPagedResultDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProtocolPagedResultDTO>> GetPagedAsync(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 15,
    [FromQuery] string? sortLabel = null,
    [FromQuery] string? sortDirection = null,
    [FromQuery] string? searchString = null)
    {
        _logger.LogInformation<Protocol>(
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
            ProtocolPagedResultDTO result =
                await _protocolService.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    sortLabel,
                    sortDirection,
                    searchString
                );

            if (result.Items == null || result.Items.Count == 0)
            {
                _logger.LogWarning<Protocol>(
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
                _logger.LogInformation<Protocol>(
                    message: LogSuccessMessages.FoundPaged,
                    args:
                    [
                        result.Items.Count,
                        result.TotalCount,
                    ]);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError<Protocol>(
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
               value: new ErrorResponse("Ocorreu um erro inesperado ao buscar protocolos paginados.")
            );
        }
    }

    /// <summary>
    /// Obtém a quantidade total de protocolos cadastrados.
    /// </summary>
    /// <param name="searchString">Filtro opcional para restringir a contagem.</param>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com o total de protocolos encontrados.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao contar os protocolos.</exception>
    /// <remarks>
    /// Ação: <b>Contar protocolos</b>.  
    /// - Retorna 200 (OK) com a contagem.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetTotalCountAsync([FromQuery] string? searchString = null)
    {
        _logger.LogInformation<Protocol>(
            message: LogInfoMessages.CountRequest,
            args: searchString);

        try
        {
            int total =
                await _protocolService.GetTotalProtocolsCountAsync(searchString);

            _logger.LogInformation<Protocol>(
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
            _logger.LogError<Protocol>(
                exception: ex,
                message: LogErrorMessages.CountError,
                args: searchString);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse("Ocorreu um erro inesperado ao contar os protocolos.")
            );
        }
    }

    /// <summary>
    /// Atualiza parcialmente os dados de um protocolo existente.
    /// </summary>
    /// <param name="id">Identificador único do protocolo.</param>
    /// <param name="protocolDTO">Objeto contendo os novos dados do protocolo.</param>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com o protocolo atualizado,
    /// <see cref="NotFoundObjectResult"/> caso o protocolo não exista
    /// ou <see cref="BadRequestObjectResult"/> caso os dados fornecidos sejam inválidos.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando os dados fornecidos são inválidos (ex: número duplicado, campos obrigatórios ausentes).</exception>
    /// <exception cref="Exception">Erro inesperado ao atualizar o protocolo.</exception>
    /// <remarks>
    /// Ação: <b>Atualizar protocolo</b>.  
    /// - Retorna 200 (OK) em caso de sucesso.  
    /// - Retorna 404 (Not Found) se o protocolo não for encontrado.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(ProtocolResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProtocolResponseDTO>> UpdateAsync(Guid id, [FromBody] ProtocolUpdateDTO protocolDTO)
    {
        _logger.LogInformation<Protocol>(
            message: LogInfoMessages.UpdateRequest,
            args:
            [
                id,
                protocolDTO
            ]);

        try
        {
            Protocol? entity =
                await _protocolService.UpdateAsync(id, protocolDTO);

            if (entity == null)
            {
                _logger.LogWarning<Protocol>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum protocolo encontrado para o ID {id}."));
            }

            _logger.LogInformation<Protocol>(
                message: LogSuccessMessages.Updated,
                args: id);

            return Ok(ToResponse(entity));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<Protocol>(
                exception: ex,
                message: LogWarningMessages.InvalidUpdate,
                args:
                [
                    id,
                    protocolDTO
                ]);

            return BadRequest(
                error: new ErrorResponse("Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<Protocol>(
                exception: ex,
                message: LogErrorMessages.UpdateError,
                args:
                [
                    id,
                    protocolDTO
                ]);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse("Ocorreu um erro inesperado ao atualizar o usuário.")
            );
        }
    }

    /// <summary>
    /// Exclui um protocolo pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do protocolo.</param>
    /// <returns>
    /// Retorna <see cref="NoContentResult"/> se o protocolo for excluído com sucesso,  
    /// <see cref="NotFoundObjectResult"/> se o protocolo não existir,  
    /// ou <see cref="ConflictObjectResult"/> se estiver arquivado ou com status ainda não finalizado.  
    /// </returns>
    /// <exception cref="InvalidOperationException">Protocolo não pode ser excluído devido a status não atualizado ou arquivamento.</exception>
    /// <exception cref="Exception">Erro inesperado ao excluir o protocolo.</exception>
    /// <remarks>
    /// Ação: <b>Excluir protocolo</b>.  
    /// 
    ///  **Regras de Negócio:** Um protocolo não pode ser excluído se estiver com flag de status ainda não finalizado ou arquivado.
    ///   Neste caso, a API retornará um status 409 (Conflict) com uma mensagem explicativa.
    /// - Retorna 204 (No Content) em caso de exclusão bem-sucedida.  
    /// - Retorna 404 (Not Found) se o protocolo não for encontrado.  
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
        _logger.LogInformation<Protocol>(
            message: LogInfoMessages.DeleteRequest,
            args: id);

        try
        {
            bool deleted =
                await _protocolService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning<Protocol>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum protocolo encontrado para o ID {id}."));
            }

            _logger.LogInformation<Protocol>(
                message: LogSuccessMessages.Deleted,
                args: id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning<Protocol>(
                exception: ex,
                message: LogWarningMessages.InvalidOperation,
                args: id);

            return Conflict(
                error: new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<Protocol>(
                exception: ex,
                message: LogErrorMessages.DeleteError,
                args: id);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse("Ocorreu um erro inesperado ao excluir o usuário.")
            );
        }

    }

    /// <summary>
    /// Converte uma entidade <see cref="Protocol"/> em um objeto de resposta padronizado <see cref="ProtocolResponseDTO"/>.
    /// </summary>
    /// <param name="entity">Entidade <see cref="Protocol"/> obtida da camada de domínio.</param>
    /// <returns>
    /// Retorna um objeto <see cref="ProtocolResponseDTO"/> contendo os dados essenciais do protocolo
    /// que serão expostos pela API.
    /// </returns>
    /// <remarks>
    /// Este método garante que apenas informações relevantes e seguras sejam retornadas aos consumidores da API,
    /// servindo como camada de mapeamento entre a entidade de domínio e o contrato de saída.
    /// 
    /// **Campos retornados:**
    /// - <c>Id</c> → Identificador único do protocolo.  
    /// - <c>Number</c> → Número sequencial para índice do protocolo.  
    /// - <c>Subject</c> → Título para assunto do protocolo.  
    /// - <c>Description</c> → Corpo (descrição) do assunto do protocolo.  
    /// - <c>Status</c> → Status de movimentação em que se encontra o protocoo (Ex: Em aberto, Em correção... ).  
    /// - <c>IsArchived</c> → Booleano que indica se o protocolo finalizado foi arquivado ou permanece em aberto para alterações.  
    /// - <c>CreatedByName</c> → Nome do usuário que gerou o protocolo.  
    /// - <c>DestinationUserName</c> → Nome de usuário ao qual se destina o protocolo.  
    /// - <c>CreatedAt</c> → Data de criação do protocolo.  
    /// - <c>OriginSectorAcronym</c> → Abreviação de nome do setor de origem ao qual o usuário que gerou o protocolo pertence.  
    /// - <c>DestinationSectorAcronym</c> → Abreviação de nome do setor de destino ao qual o protocolo é direcionado.  
    /// - <c>UpdatedAt</c> → Data da última atualização do registro (se houver).  
    /// - <c>UpdatedByName</c> → Nome do último usuário ao realizar alterações no registro de protocolo.  
    /// </remarks>
    private static ProtocolResponseDTO ToResponse(Protocol entity) => new()
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
}