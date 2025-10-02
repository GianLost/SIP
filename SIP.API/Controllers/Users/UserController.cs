using Microsoft.AspNetCore.Mvc;
using SIP.API.Controllers.Errors;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.DTOs.Users.Default;
using SIP.API.Domain.DTOs.Users.Pagination;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Helpers.Extensions;
using SIP.API.Domain.Helpers.Messages.LogMessage.Error;
using SIP.API.Domain.Helpers.Messages.LogMessage.Info;
using SIP.API.Domain.Helpers.Messages.LogMessage.Success;
using SIP.API.Domain.Helpers.Messages.LogMessage.Warning;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Users.Configurations;

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

    private readonly IUser _userService = user;
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
                await _userService.CreateAsync(userDTO);

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
                error: new ErrorResponse("Invalid data: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.CreateError,
                args: userDTO);

            return StatusCode(
                statusCode: StatusCodes.Status500InternalServerError, 
                value: new ErrorResponse("Ocorreu um erro inesperado."));
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
    public async Task<ActionResult<UserResponseDTO>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.GetByIdRequest,
            args: id);
        try
        {
            User? user =
                await _userService.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum Usuário encontrado para o ID {id}"));
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.FoundById,
                args: id);

            UserResponseDTO response = new()
            {
                Id = user.Id,
                Name = user.Name,
                Login = user.Login,
                Masp = user.Masp,
                Email = user.Email,
                Role = user.Role,
                Status = user.IsActive == true ? "Active" : "Inactive",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                SectorId = user.SectorId,
                ProtocolsCreated = user.ProtocolsCreated,
                ProtocolsReceived = user.ProtocolsReceived
            };

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
               value: new ErrorResponse("Ocorreu um erro inesperado ao consultar o usuário pelo ID.")
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
    [ProducesResponseType(typeof(IEnumerable<UserDefaultDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserDefaultDTO>>> GetAllAsync()
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.GetAllRequest);

        try
        {
            ICollection<User> users = 
                await _userService.GetAllAsync();

            if(users == null || users.Count == 0)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.Empty);

                return Ok(Enumerable.Empty<UserDefaultDTO>());
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.FoundAll,
                args: users.Count);

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError<User>(
                exception: ex,
                message: LogErrorMessages.GetAllError);

            return StatusCode(
               statusCode: StatusCodes.Status500InternalServerError,
               value: new ErrorResponse("Ocorreu um erro inesperado ao buscar todos os usuários.")
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
    [ProducesResponseType(typeof(UserPagedResultDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserPagedResultDTO>> GetPagedAsync(
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
            UserPagedResultDTO result =
            await _userService.GetPagedAsync(
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
               value: new ErrorResponse("Ocorreu um erro inesperado ao buscar usuários paginados.")
            );
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
                await _userService.GetTotalUsersCountAsync(searchString);

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
               value: new ErrorResponse("Ocorreu um erro inesperado ao contar os usuários.")
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
                await _userService.UpdateAsync(id, userDTO);

            if (updated == null)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum usuário encontrado para o ID {id}."));
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
                error: new ErrorResponse("Invalid data: " + ex.Message));
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
               value: new ErrorResponse("Ocorreu um erro inesperado ao atualizar o usuário.")
            );
        }
    }

    /// <summary>
    /// Altera a senha de um usuário utilizando o fluxo administrativo.
    /// </summary>
    /// <param name="dto">
    /// Objeto <see cref="UserDefaultChangePasswordDTO"/> contendo o identificador do usuário (<c>Id</c>) 
    /// e a nova senha (<c>Password</c>).
    /// </param>
    /// <returns>
    /// Retorna:
    /// - <see cref="OkObjectResult"/> com uma mensagem de confirmação caso a senha seja alterada com sucesso.  
    /// - <see cref="BadRequestObjectResult"/> se os dados fornecidos forem inválidos.  
    /// - <see cref="NotFoundObjectResult"/> se o usuário não for encontrado.  
    /// - <see cref="ObjectResult"/> (500) em caso de erro inesperado.  
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando os dados fornecidos são inválidos (ex: senha não fornecida).</exception>
    /// <exception cref="Exception">Erro inesperado durante o processo de alteração da senha.</exception>
    /// <remarks>
    /// Ação: <b>Alterar senha do usuário (fluxo administrativo)</b>.  
    /// - Este endpoint permite que um administrador altere a senha de um usuário sem precisar da senha atual.  
    /// - A nova senha será devidamente criptografada antes de ser armazenada.  
    /// - Retorna 200 (OK) com mensagem de sucesso.  
    /// - Retorna 400 (Bad Request) quando a requisição é inválida.  
    /// - Retorna 404 (Not Found) se o usuário não existir.  
    /// - Retorna 500 (Internal Server Error) em caso de falha inesperada.  
    /// </remarks>
    [HttpPatch("password")]
    [ProducesResponseType(typeof(ChangedPasswordResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DefaultChangePasswordAsync([FromBody] UserDefaultChangePasswordDTO dto)
    {
        _logger.LogInformation<User>(
            message: LogInfoMessages.DefaultChangePasswordRequest,
            args: dto.Id);

        try
        {
            User? updated =
                await _userConfigurationService.DefaultChangePasswordAsync(dto);

            if (updated == null)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: dto.Id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum usuário encontrado para o ID {dto.Id}."));
            }

            _logger.LogInformation<User>(
                message: LogSuccessMessages.PasswordChanged,
                args: updated.Id);

            ChangedPasswordResponseDTO response =
                new($"Senha alterada com sucesso para o usuário {updated.Login}.");

            return Ok(response);
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
                value: new ErrorResponse("Ocorreu um erro inesperado ao alterar a senha do usuário."));
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
                await _userService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning<User>(
                    message: LogWarningMessages.NotFound,
                    args: id);

                return NotFound(
                    value: new ErrorResponse($"Nenhum usuário encontrado para o ID {id}."));
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
               value: new ErrorResponse("Ocorreu um erro inesperado ao excluir o usuário.")
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
    /// - <c>Id</c> → Identificador único do setor.  
    /// - <c>Name</c> → Nome do setor.  
    /// - <c>Acronym</c> → Sigla ou abreviação do setor.  
    /// - <c>Phone</c> → Telefone de contato do setor.  
    /// - <c>CreatedAt</c> → Data de criação do registro.  
    /// - <c>UpdatedAt</c> → Data da última atualização do registro (se houver).  
    /// </remarks>
    private static UserResponseDTO ToResponse(User entity) => new()
    {
        Id = entity.Id,
        Masp = entity.Masp,
        Name = entity.Name,
        Login = entity.Login,
        Email = entity.Email,
        Status = entity.IsActive == true ? "Active" : "Inactive",
        Role = entity.Role,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        SectorId = entity.SectorId
    };
}