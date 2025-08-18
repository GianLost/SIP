using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Users.Configurations;

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

            UserReponseDTO response = new()
            {
                FullName = entity.FullName,
                Login = entity.Login,
                Masp = entity.Masp,
                Email = entity.Email,
                CreatedAt = entity.CreatedAt
            };

            return CreatedAtRoute(nameof(GetUserByIdAsync), new { id = entity.Id }, response);
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
    [HttpGet("{id}", Name = "GetUserByIdAsync")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByIdAsync(Guid id)
    {
        User? user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Retrieves all users records.
    /// </summary>
    /// Returns an <see cref="IActionResult"/> containing a list of users, wrapped in an HTTP 200 OK response.
    [HttpGet("show")]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        ICollection<User> sectors = await _userService.GetAllAsync();
        return Ok(sectors);
    }

    /// <summary>
    /// Gets a paginated result of users from the API, including total count. Use in-memory caching and limit the number of records per page to avoid multiple requests for the same data.
    /// </summary>
    /// <param name="pageNumber">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="sortLabel">The property name to sort by.</param>
    /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
    /// <param name="searchString">Optional search string to filter users.</param>
    /// <returns>A paged result DTO containing the users and total count.</returns>
    [HttpGet("show_paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 15, [FromQuery] string? sortLabel = null, [FromQuery] string? sortDirection = null, [FromQuery] string? searchString = null)
    {
        UserPagedResultDTO result = await _userService.GetPagedAsync(pageNumber, pageSize, sortLabel, sortDirection, searchString);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the total number of users that match the given search criteria.
    /// </summary>
    /// <param name="searchString">A keyword used to filter users by name or other relevant fields (optional).</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing the total count of users as an integer, wrapped in an HTTP 200 OK response.
    /// </returns>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalCountAsync([FromQuery] string? searchString = null)
    {
        int total = await _userService.GetTotalUsersCountAsync(searchString);
        return Ok(total);
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
    [HttpPatch("default_update_user/{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UserUpdateDTO userDTO)
    {

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        User? updated = await _userService.UpdateAsync(id, userDTO);

        if (updated == null)
            return NotFound();

        UserReponseDTO response = new()
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
    /// Changes a specific user's password using an administrative flow.
    /// </summary>
    /// <remarks>
    /// This endpoint allows changing a user's password without knowing their current one. 
    /// The new password will be securely hashed before being stored.
    /// </remarks>
    /// <param name="dto">The request body containing the `UserId` and the new `Password`.</param>
    /// <response code="200">Password was changed successfully. A confirmation message is returned.</response>
    /// <response code="400">The request is invalid. This can be due to a missing password or other model validation errors.</response>
    /// <response code="404">No user was found with the provided `UserId`.</response>
    /// <response code="500">An internal server error occurred.</response>
    [HttpPatch("default-change-password")]
    [ProducesResponseType(typeof(ChangedPasswordResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DefaultChangePasswordAsync([FromBody] UserDefaultChangePasswordDTO dto)
    {
        try
        {
            User? updatedUser = await _userConfigurationService.DefaultChangePasswordAsync(dto);

            if (updatedUser == null)
                return NotFound("User not found.");

            ChangedPasswordResponseDTO response = new($"Password successfully changed for user {updatedUser.Login}.");

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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
            bool deleted = await _userService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { Message = "User was deleted with success." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict( new ErrorResponse(ex.Message));
        }

    }
}