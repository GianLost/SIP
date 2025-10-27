using Microsoft.AspNetCore.Mvc;
using SIP.API.Controllers.Errors;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Users.Configurations;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Helpers.Extensions;
using SIP.API.Domain.DTOs.Default.Pagination;
using SIP.API.Domain.DTOs.Users.Pagination;
using SIP.API.Domain.DTOs.Protocols.Default;
using SIP.API.Domain.Helpers.Messages.LogMessages.Default.Error;
using SIP.API.Domain.Helpers.Messages.LogMessages.Default.Warning;
using SIP.API.Domain.Helpers.Messages.LogMessages.Default.Info;
using SIP.API.Domain.Helpers.Messages.LogMessages.Default.Success;

namespace SIP.API.Controllers.Users;

/// <summary>
/// Controller responsável por gerenciar operações relacionadas aos <c>Usuários</c>.
/// </summary>
/// <remarks>
/// Um <c>Usuário</c> representa o objeto principal de gerenciamento da aplicação.
/// Esta controller fornece endpoints para criação, consulta, atualização, manutenção e exclusão de usuários.
/// </remarks>
[Route("sip_api/users")]
[ApiController]
public class UserController(IUser user, IUserConfiguration userConfiguration, ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserConfiguration _userConfigurationService = userConfiguration;
    private readonly ILogger<UserController> _logger = logger;

    /// <summary>
    /// Cria um novo usuário no sistema.
    /// </summary>
    /// <param name="userDTO">Objeto contendo os dados necessários para criar um usuário.</param>
    /// <returns>
    /// Retorna <see cref="CreatedAtActionResult"/> com o usuário criado e o cabeçalho <c>Location</c>,
    /// ou <see cref="BadRequestObjectResult"/> caso os dados fornecidos sejam inválidos.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando os dados fornecidos são inválidos (ex: nome duplicado, campos obrigatórios ausentes).</exception>
    /// <exception cref="Exception">Erro inesperado durante o processo de criação.</exception>
    /// <remarks>
    /// Ação: <b>Criar usuário</b>.  
    /// - Retorna 201 (Created) em caso de sucesso.  
    /// - Retorna 400 (Bad Request) quando os dados não são válidos.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDTO>> CreateAsync([FromBody] UserCreateDTO userDTO)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.CreateRequest,
            args: userDTO);

        try
        {
            User entity =
                await user.CreateAsync(userDTO);

            _logger.LogInformation<User>(
            message: LogSuccessMessages.Created,
            args:
            [
                entity.Id,
                entity.Name,
                entity.CreatedAt
            ]);

            return CreatedAtRoute(
                routeName: "GetUserByIdAsync",
                routeValues: new { id = entity.Id },
                value: ToResponse(entity));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<User>(
                exception: ex,
                message: LogWarningMessages.InvalidCreate,
                args: userDTO);

            return BadRequest(
                error: new ErrorResponse(
                    error: "Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.CreateError,
                args: userDTO);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: new ErrorResponse(
                    error: "Ocorreu um erro inesperado."));
        }
    }

    /// <summary>
    /// Obtém um usuário pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único (GUID) do usuário.</param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com os dados do usuário, se encontrado.  
    /// - <see cref="NotFoundObjectResult"/> caso o usuário não exista.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao consultar o usuário.</exception>
    /// <remarks>
    /// Ação: <b>Consultar usuário por ID</b>.  
    /// - Retorna 200 (OK) com os dados do usuário.  
    /// - Retorna 404 (Not Found) se o usuário não existir.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet("{id}", Name = "GetUserByIdAsync")]
    [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResultDTO<UserResponseDTO>>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.GetByIdRequest,
            args: id);
        try
        {
            PagedResultDTO<UserBasicListDTO> response =
                await user.GetByIdAsync(id);

            if (response == null)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse(
                        error: $"Nenhum usuário encontrado para o ID {id}"));
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.FoundById,
                args: id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.GetByIdError,
                args: id);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse(
                   error: "Ocorreu um erro inesperado ao consultar o usuário pelo ID.")
            );
        }
    }

    /// <summary>
    /// Obtém todos os usuários cadastrados.
    /// </summary>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com a lista de usuários.
    /// Se não houver registros, retorna uma lista vazia.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao buscar todos os usuários.</exception>
    /// <remarks>
    /// Ação: <b>Listar todos os usuários</b>.  
    /// - Retorna 200 (OK) com a lista de usuários.  
    /// - Retorna lista vazia se não houver usuários cadastrados.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PagedResultDTO<UserResponseDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PagedResultDTO<UserResponseDTO>>>> GetAllAsync()
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.GetAllRequest);

        try
        {
            PagedResultDTO<UserResponseDTO> result =
                await user.GetAllAsync();

            if (result.Items == null || result.Items.Count == 0)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.Empty);

                return Ok(Enumerable.Empty<UserResponseDTO>());
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.FoundAll,
                args: result.Items.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.GetAllError);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse(
                   error: "Ocorreu um erro inesperado ao buscar todos os usuários.")
            );
        }


    }

    /// <summary>
    /// Obtém usuários de forma paginada.
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
    [ProducesResponseType(typeof(PagedResultDTO<UserListItemDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResultDTO<UserListItemDTO>>> GetPagedAsync(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 15,
    [FromQuery] string? sortLabel = null,
    [FromQuery] string? sortDirection = null,
    [FromQuery] string? searchString = null)
    {
        _logger.LogInformation<User>(
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
            PagedResultDTO<UserBasicListDTO> result =
                await user.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    sortLabel,
                    sortDirection,
                    searchString
                );

            if (result.Items == null || result.Items.Count == 0)
            {
                _logger.LogWarning<User>(
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
                _logger.LogInformation<User>(
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
            _logger.LogError<User>(
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
               value: new ErrorResponse(
                   error: "Ocorreu um erro inesperado ao buscar usuários paginados.")
            );
        }
    }

    /// <summary>
    /// Retorna a lista de protocolos criados por um usuário específico.
    /// </summary>
    /// <param name="userId">
    /// Identificador único (<see cref="Guid"/>) do usuário cujos protocolos criados serão retornados.
    /// </param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com a lista (possivelmente vazia) de protocolos criados pelo usuário.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <remarks>
    /// <b>Ação:</b> Consultar protocolos criados por um usuário.  
    /// 
    /// Este endpoint retorna todos os protocolos cadastrados pelo usuário informado.
    /// Caso o usuário ainda não tenha criado nenhum protocolo, uma lista vazia é retornada
    /// para manter a consistência da resposta esperada pelo cliente.  
    /// 
    /// 🔄 <b>Retornos possíveis:</b>
    /// - <b>200 (OK)</b> → Lista de protocolos retornada com sucesso (mesmo que vazia).  
    /// - <b>500 (Internal Server Error)</b> → Erro inesperado ao processar a solicitação.  
    /// </remarks>
    [HttpGet("{userId:guid}/protocols_created")]
    [ProducesResponseType(typeof(List<ProtocolDefaultDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProtocolDefaultDTO>>> GetCreatedProtocolsByUserAsync(Guid userId)
    {
        _logger.LogInformation<User>(
            message: "Solicitação para buscar protocolos criados pelo usuário {UserId}.",
            args: userId);

        try
        {
            List<ProtocolDefaultDTO> protocols =
                await user.GetCreatedProtocolsByUserAsync(userId);

            if (protocols == null || protocols.Count == 0)
            {
                _logger.LogInformation<User>(
                    message: "Nenhum protocolo encontrado para o usuário {UserId}.",
                    args: userId);

                // Retorna lista vazia em vez de erro
                return Ok(new List<ProtocolDefaultDTO>());
            }

            _logger.LogInformation<User>(
                message: "Protocolos criados pelo usuário {UserId} retornados com sucesso.",
                args: userId);

            return Ok(protocols);
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: "Erro inesperado ao buscar protocolos criados pelo usuário {UserId}.",
                args: userId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocorreu um erro inesperado ao consultar os protocolos do usuário."));
        }
    }

    /// <summary>
    /// Retorna os detalhes completos de um usuário específico.
    /// </summary>
    /// <param name="id">
    /// Identificador único (<see cref="Guid"/>) do usuário cujos detalhes serão consultados.
    /// </param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com os detalhes do usuário, se encontrado.  
    /// - <see cref="NotFoundObjectResult"/> se o usuário não for localizado.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <remarks>
    /// <b>Ação:</b> Consultar detalhes de um usuário.  
    /// 
    /// Este endpoint retorna as informações completas de um usuário específico,
    /// incluindo dados cadastrais e metadados associados.  
    /// 
    /// 🔄 <b>Retornos possíveis:</b>
    /// - <b>200 (OK)</b> → Detalhes do usuário retornados com sucesso.  
    /// - <b>404 (Not Found)</b> → Usuário não encontrado.  
    /// - <b>500 (Internal Server Error)</b> → Erro inesperado ao consultar os detalhes do usuário.  
    /// </remarks>
    [HttpGet("{id:guid}/details")]
    [ProducesResponseType(typeof(PagedResultDTO<UserListItemDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResultDTO<UserListItemDTO>>> GetDetailsAsync(Guid id)
    {
        _logger.LogInformation<User>(
        message: "Solicitação para buscar detalhes do usuário {UserId}.",
        args: id);

        try
        {
            PagedResultDTO<UserListItemDTO> result =
                await user.GetDetailsAsync(id);

            if (result.Items == null || result.Items.Count == 0)
            {
                _logger.LogWarning<User>(
                    message: "Nenhum detalhe encontrado para o usuário {UserId}.",
                    args: id);

                return NotFound(new ErrorResponse(
                    error: $"Usuário não encontrado para o ID {id}."));
            }

            _logger.LogInformation<User>(
                message: "Detalhes do usuário {UserId} retornados com sucesso.",
                args: id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: "Erro inesperado ao buscar detalhes do usuário {UserId}.",
                args: id);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocorreu um erro inesperado ao consultar os detalhes do usuário."));
        }
    }

    /// <summary>
    /// Obtém a quantidade total de usuários cadastrados.
    /// </summary>
    /// <param name="searchString">Filtro opcional para restringir a contagem.</param>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com o total de usuários encontrados.
    /// </returns>
    /// <exception cref="Exception">Erro inesperado ao contar os usuários.</exception>
    /// <remarks>
    /// Ação: <b>Contar usuários</b>.  
    /// - Retorna 200 (OK) com a contagem.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetTotalCountAsync([FromQuery] string? searchString = null)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.CountRequest,
            args: searchString);

        try
        {
            int total =
                await user.GetTotalCountAsync(searchString);

            _logger.LogInformation<User>(
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
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.CountError,
                args: searchString);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse(
                   error: "Ocorreu um erro inesperado ao contar os usuários.")
            );
        }
    }

    /// <summary>
    /// Atualiza parcialmente os dados de um usuário existente.
    /// </summary>
    /// <param name="id">Identificador único do usuário.</param>
    /// <param name="userDTO">Objeto contendo os novos dados do usuário.</param>
    /// <returns>
    /// Retorna <see cref="OkObjectResult"/> com o usuário atualizado,
    /// <see cref="NotFoundObjectResult"/> caso o usuário não exista
    /// ou <see cref="BadRequestObjectResult"/> caso os dados fornecidos sejam inválidos.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando os dados fornecidos são inválidos (ex: nome duplicado, campos obrigatórios ausentes).</exception>
    /// <exception cref="Exception">Erro inesperado ao atualizar o usuário.</exception>
    /// <remarks>
    /// Ação: <b>Atualizar usuário</b>.  
    /// - Retorna 200 (OK) em caso de sucesso.  
    /// - Retorna 404 (Not Found) se o usuário não for encontrado.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDTO>> UpdateAsync(Guid id, [FromBody] UserUpdateDTO userDTO)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.UpdateRequest,
            args:
            [
                id,
                userDTO
            ]);

        try
        {
            User? updated =
                await user.UpdateAsync(id, userDTO);

            if (updated == null)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse(
                        error: $"Nenhum usuário encontrado para o ID {id}."));
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.Updated,
                args: id);

            return Ok(ToResponse(updated));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<User>(
                exception: ex,
                message: LogWarningMessages.InvalidUpdate,
                args:
                [
                    id,
                    userDTO
                ]);

            return BadRequest(
                error: new ErrorResponse(
                    error: "Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.UpdateError,
                args:
                [
                    id,
                    userDTO
                ]);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse(
                   error: "Ocorreu um erro inesperado ao atualizar o usuário.")
            );
        }
    }

    /// <summary>
    /// Atualiza o setor vinculado a um usuário existente (fluxo administrativo).
    /// </summary>
    /// <param name="id">Identificador único do usuário cujo setor será atualizado.</param>
    /// <param name="dto">
    /// Objeto <see cref="UserResponseDTO"/> contendo o identificador do novo se(<c>SectorId</c>).
    /// </param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com uma mensagem de confirmação casoosetorsejaatualizado com sucesso.  
    /// - <see cref="BadRequestObjectResult"/> se os dados fornecidos forem inválidos.  
    /// - <see cref="NotFoundObjectResult"/> se o usuário ou o setorinformadonãoforemencontrados.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando o identificador do usuário ou do setor é inválido.
    /// </exception>
    /// <exception cref="Exception">
    /// Lançada em caso de erro inesperado durante o processo de atualização.
    /// </exception>
    /// <remarks>
    /// <b>Ação:</b> Atualizar setor do usuário (fluxo administrativo).  
    /// 
    /// Este endpoint permite que um **administrador** altere o setor vinculado a um usuário específico, 
    /// de acordo com movimentações internas ou ajustes administrativos.  
    /// 
    /// ⚙️ Comportamento:
    /// - Verifica a existência do usuário e do setor antes da atualização.  
    /// - Atualiza o campo <c>UpdatedAt</c> automaticamente.  
    /// 
    /// 🔄 Retornos possíveis:
    /// - <b>200 (OK)</b> → Setor atualizado com sucesso.  
    /// - <b>400 (Bad Request)</b> → Dados inválidos.  
    /// - <b>404 (Not Found)</b> → Usuário ou setor não encontrados.  
    /// - <b>500 (Internal Server Error)</b> → Erro inesperado.  
    /// </remarks>
    [HttpPatch("{id:guid}/sector")]
    [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserSectorAsync(Guid id, [FromBody] UserChangeSectorDTO dto)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.UpdateRequest,
            args: dto.UserId);

        try
        {
            User? updated = await _userConfigurationService.ReassignUserSectorAsync(id, dto);

            if (updated == null)
                return NotFound(new ErrorResponse(
                    error: $"Usuário não encontrado para o ID {dto.UserId}."));

            _logger.LogInformation<User>(
                message: LogSuccessMessages.Updated,
                args: updated.Id);

            return Ok(ToResponse(updated));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<User>(ex, LogWarningMessages.InvalidUpdate, dto);
            return BadRequest(new ErrorResponse(
                error: "Dados inválidos: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(ex, LogErrorMessages.UpdateError, dto);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse(
                    error: "Ocorreu um erro inesperado ao alterar o setor do usuário."));
        }
    }

    /// <summary>
    /// Altera a senha de um usuário utilizando o fluxo administrativo.
    /// </summary>
    /// <param name="id">
    /// Identificador único (<see cref="Guid"/>) do usuário cuja senha será redefinida.
    /// </param>
    /// <param name="dto">
    /// Objeto <see cref="AdminResetPasswordDTO"/> contendo a nova senha (<c>Password</c>) 
    /// e sua confirmação (<c>ConfirmPassword</c>).
    /// </param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com uma mensagem de confirmação caso a senha seja alterada com sucesso.  
    /// - <see cref="BadRequestObjectResult"/> se os dados fornecidos forem inválidos.  
    /// - <see cref="NotFoundObjectResult"/> se o usuário não for encontrado.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando a nova senha não é fornecida ou os dados do DTO são inválidos.
    /// </exception>
    /// <exception cref="Exception">
    /// Lançada em caso de erro inesperado durante o processo de redefinição da senha.
    /// </exception>
    /// <remarks>
    /// <b>Ação:</b> Alterar senha de um usuário (fluxo administrativo).  
    /// 
    /// Este endpoint permite que um **administrador** altere a senha de qualquer usuário, 
    /// sem a necessidade de informar a senha atual.  
    /// 
    /// ⚙️ Comportamento:
    /// - A nova senha será criptografada antes de ser persistida no banco de dados.  
    /// - Atualiza o campo <c>UpdatedAt</c> do usuário.  
    /// 
    /// 🔄 Retornos possíveis:
    /// - <b>200 (OK)</b> → Senha alterada com sucesso.  
    /// - <b>400 (Bad Request)</b> → Dados inválidos ou senha não informada.  
    /// - <b>404 (Not Found)</b> → Usuário não encontrado.  
    /// - <b>500 (Internal Server Error)</b> → Erro inesperado.  
    /// </remarks>
    [HttpPatch("password/{id:guid}")]
    [ProducesResponseType(typeof(ChangedPasswordResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPasswordByAdminAsync(
        Guid id,
        [FromBody] AdminResetPasswordDTO dto)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.DefaultChangePasswordRequest,
            args: id);

        try
        {
            User? updated =
                await _userConfigurationService.ResetPasswordByAdminAsync(id, dto);

            if (updated == null)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse(
                        error: $"Nenhum usuário encontrado para o ID {id}."));
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.PasswordChanged,
                args: updated.Id);

            return Ok(new ChangedPasswordResponseDTO(
                message: $"Senha redefinida com sucesso para o usuário {updated.Login}."));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<User>(
                exception: ex,
                message: LogWarningMessages.InvalidUpdate,
                args: dto);

            return BadRequest(
                error: new ErrorResponse("Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.UpdateError,
                args: dto);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError,
                value: new ErrorResponse(
                    error: "Ocorreu um erro inesperado ao redefinir a senha do usuário."));
        }
    }

    /// <summary>
    /// Permite que o próprio usuário altere sua senha, mediante validação da senha atual.
    /// </summary>
    /// <param name="dto">
    /// Objeto <see cref="UserChangePasswordDTO"/> contendo a senha atual (<c>CurrentPassword</c>), 
    /// a nova senha (<c>NewPassword</c>) e a confirmação da nova senha (<c>ConfirmPassword</c>).
    /// </param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com uma mensagem de confirmação caso a senha seja alterada com sucesso.  
    /// - <see cref="BadRequestObjectResult"/> se as senhas não coincidirem ou se os dados forem inválidos.  
    /// - <see cref="NotFoundObjectResult"/> se o usuário não for encontrado.  
    /// - <see cref="UnauthorizedObjectResult"/> se a senha atual estiver incorreta.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando os dados de entrada estão ausentes ou inconsistentes.
    /// </exception>
    /// <exception cref="Exception">
    /// Lançada em caso de erro inesperado durante o processo de alteração da senha.
    /// </exception>
    /// <remarks>
    /// <b>Ação:</b> Alterar senha pessoal (fluxo de usuário comum).  
    /// 
    /// Este endpoint permite que o **usuário autenticado** altere sua própria senha, 
    /// desde que informe corretamente a senha atual.  
    /// 
    /// ⚙️ Comportamento:
    /// - A senha atual é validada com base no hash armazenado.  
    /// - A nova senha é criptografada e substitui a anterior.  
    /// - Atualiza o campo <c>UpdatedAt</c> do usuário.  
    /// 
    /// 🔄 Retornos possíveis:
    /// - <b>200 (OK)</b> → Senha alterada com sucesso.  
    /// - <b>400 (Bad Request)</b> → Dados inválidos ou senha nova não informada.  
    /// - <b>401 (Unauthorized)</b> → Senha atual incorreta.  
    /// - <b>404 (Not Found)</b> → Usuário não encontrado.  
    /// - <b>500 (Internal Server Error)</b> → Erro inesperado.  
    /// </remarks>
    [HttpPatch("password")]
    [ProducesResponseType(typeof(ChangedPasswordResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeOwnPasswordAsync([FromBody] UserChangePasswordDTO dto)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.UpdateRequest,
            args: dto.Id);

        try
        {
            User? updated = await _userConfigurationService.ChangeOwnPasswordAsync(dto);

            if (updated == null)
                return BadRequest(new ErrorResponse(
                    error: "Senha atual incorreta ou dados inválidos."));

            _logger.LogInformation<User>(
                message: LogSuccessMessages.PasswordChanged,
                args: updated.Id);

            return Ok(new ChangedPasswordResponseDTO(
                message: "Senha alterada com sucesso."));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning<User>(ex, LogWarningMessages.InvalidUpdate, dto);
            return BadRequest(new ErrorResponse(
                error: "Dados inválidos: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(ex, LogErrorMessages.UpdateError, dto);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse(
                    error: "Ocorreu um erro inesperado ao alterar a senha do usuário."));
        }
    }

    /// <summary>
    /// Exclui um usuário pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do usuário.</param>
    /// <returns>
    /// Retorna <see cref="NoContentResult"/> se o usuário for excluído com sucesso,  
    /// <see cref="NotFoundObjectResult"/> se o usuário não existir,  
    /// ou <see cref="ConflictObjectResult"/> se houver entidades vinculadas (ex: protocolos).  
    /// </returns>
    /// <exception cref="InvalidOperationException">Usuário não pode ser excluído devido a vínculos.</exception>
    /// <exception cref="Exception">Erro inesperado ao excluir o usuário.</exception>
    /// <remarks>
    /// Ação: <b>Excluir usuário</b>.  
    /// 
    ///  **Regras de Negócio:** Um usuário não pode ser excluído se houver entidades associadas a ele (ex: protocolos).
    ///   Neste caso, a API retornará um status 409 (Conflict) com uma mensagem explicativa.
    /// - Retorna 204 (No Content) em caso de exclusão bem-sucedida.  
    /// - Retorna 404 (Not Found) se o usuário não for encontrado.  
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
        _logger.LogInformation<User>(
            message: LogInfoMessages.DeleteRequest,
            args: id);

        try
        {
            bool deleted =
                await user.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse(
                        error: $"Nenhum usuário encontrado para o ID {id}."));
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.Deleted,
                args: id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning<User>(
                exception: ex,
                message: LogWarningMessages.InvalidOperation,
                args: id);

            return Conflict(
                error: new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.DeleteError,
                args: id);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse(
                   error: "Ocorreu um erro inesperado ao excluir o usuário.")
            );
        }

    }

    /// <summary>
    /// Converte uma entidade <see cref="User"/> em um objeto de resposta padronizado <see cref="UserResponseDTO"/>.
    /// </summary>
    /// <param name="entity">Entidade <see cref="User"/> obtida da camada de domínio.</param>
    /// <returns>
    /// Retorna um objeto <see cref="UserResponseDTO"/> contendo os dados essenciais do setor
    /// que serão expostos pela API.
    /// </returns>
    /// <remarks>
    /// Este método garante que apenas informações relevantes e seguras sejam retornadas aos consumidores da API,
    /// servindo como camada de mapeamento entre a entidade de domínio e o contrato de saída.
    /// 
    /// **Campos retornados:**
    /// - <c>Id</c> → Identificador único do usuário.  
    /// - <c>Masp</c> → Número de identificação do usuário também utilizado como índice de consulta.  
    /// - <c>Name</c> → Nome do usuário.  
    /// - <c>Login</c> → nome de usuário personalizado utilizado para abrir sessão na aplicação.  
    /// - <c>Status</c> → Indica se o usuário está com a conta ativa ou inativa.  
    /// - <c>Role</c> → Representa o nível de acesso e permissões do usuário dentro do sistema.  
    /// - <c>CreatedAt</c> → Data de criação do registro.  
    /// - <c>UpdatedAt</c> → Data da última atualização do registro (se houver).  
    /// - <c>SectorId</c> → Identificador (chave estrangeira) que faz referência ao setor do usuário.  
    /// </remarks>
    private static UserResponseDTO ToResponse(User entity) => new()
    {
        Id = entity.Id,
        Status = entity.IsActive,
        Masp = entity.Masp,
        Name = entity.Name,
        Login = entity.Login,
        Email = entity.Email,
        SectorId = entity.SectorId
    };
}