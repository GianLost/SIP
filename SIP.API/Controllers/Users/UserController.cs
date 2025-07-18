using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Users.Configurations;
using SIP.API.Domain.Services.Users.Configurations;

namespace SIP.API.Controllers.Users;

/// <summary>
/// API controller for managing user entities.
/// </summary>
[Route("sip_api/[controller]")]
[ApiController]
public class UserController(IUser user, IUserConfiguration userConfiguration) : ControllerBase
{

    private readonly IUser _userService = user;
    private readonly IUserConfiguration _userConfigurationService = userConfiguration;

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="userDTO">The data transfer object containing the user's information.</param>
    /// <returns>
    /// Returns <see cref="CreatedAtActionResult"/> with the created user and location header if successful,
    /// or <see cref="BadRequestObjectResult"/> with error details if the request is invalid.
    /// </returns>
    [HttpPost("register_user")]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] UserCreateDTO userDTO)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            User entity = await _userService.CreateAsync(userDTO);

            var response = new UserReponseDTO
            {
                FullName = entity.FullName,
                Login = entity.Login,
                Masp = entity.Masp,
                Email = entity.Email,
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
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the <see cref="User"/> if found,
    /// or <see cref="NotFoundResult"/> if no user exists with the specified ID.
    /// </returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Retrieves a paginated, optionally sorted and filtered list of users.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve. Defaults to 1.</param>
    /// <param name="pageSize">The number of users to include per page. Defaults to 20.</param>
    /// <param name="sortLabel">The field name to sort by (optional).</param>
    /// <param name="sortDirection">The direction of sorting: "asc" for ascending or "desc" for descending (optional).</param>
    /// <param name="searchString">A keyword used to filter users by name or other relevant fields (optional).</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> containing a list of sectors matching the criteria, wrapped in an HTTP 200 OK response.
    /// </returns>
    [HttpGet("show")]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 15,
    [FromQuery] string? sortLabel = null,
    [FromQuery] string? sortDirection = null,
    [FromQuery] string? searchString = null)
    {
        var sectors = await _userService.GetAllAsync(pageNumber, pageSize, sortLabel, sortDirection, searchString);
        return Ok(sectors);
    }

    /// <summary>
    /// Gets a paginated result of users from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter sectors.</param>
    /// <returns>A paged result DTO containing the users and total count.</returns>
    [HttpGet("show_paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 15, [FromQuery] string? sortLabel = null, [FromQuery] string? sortDirection = null, [FromQuery] string? searchString = null)
    {
        var result = await _userService.GetPagedAsync(pageNumber, pageSize, sortLabel, sortDirection, searchString);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the total number of users that match the given search criteria.
    /// </summary>
    /// <param name="searchString">A keyword used to filter users by name or other relevant fields (optional).</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing the total count of sectors as an integer, wrapped in an HTTP 200 OK response.
    /// </returns>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalCount([FromQuery] string? searchString = null)
    {
        var total = await _userService.GetTotalUsersCountAsync(searchString);
        return Ok(total);
    }

    // Inside SIP.API.Controllers.UserController
    [HttpPost("invalidate_count_cache")]
    public IActionResult InvalidateCountCache()
    {
        _userService.ClearTotalUsersCountCache();
        return Ok();
    }

    /// <summary>
    /// Updates an existing user by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="userDTO">The data transfer object with updated user information.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the updated <see cref="User"/> if successful,
    /// or <see cref="NotFoundResult"/> if the user does not exist.
    /// </returns>
    [HttpPut("update_user/{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UserUpdateDTO userDTO)
    {

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updated = await _userService.UpdateAsync(id, userDTO);

        if (updated == null)
            return NotFound();

        var response = new UserReponseDTO
        {
            FullName = updated.FullName,
            Login = updated.Login,
            Masp = updated.Masp,
            Email = updated.Email,
            Role = updated.Role,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Method to change user password used by managers and aministrators.
    /// </summary>
    /// <param name="dto">user's data and the new password.</param>
    /// <returns>The user updated.</returns>
    [HttpPatch("default-change-password")]
    public async Task<IActionResult> DefaultChangePassword([FromBody] UserDefaultChangePasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedUser = await _userConfigurationService.DefaultChangePasswordAsync(dto);

        if (updatedUser == null)
            return NotFound("Usuário não encontrado ou dados inválidos.");

        var response = new ChangedPasswordResponseDTO($"Senha alterada com sucesso para o usuário {updatedUser.Login}.");

        return Ok(response);
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with a success message if the user was deleted,
    /// <see cref="NotFoundResult"/> if the user does not exist,
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
            var deleted = await _userService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { Message = "User was deleted with success." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }

    }
}