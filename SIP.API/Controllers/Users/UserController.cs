using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Users;

namespace SIP.API.Controllers.Users;

/// <summary>
/// API controller for managing user entities.
/// Provides endpoints for creating, retrieving, updating, and deleting users.
/// </summary>
[Route("sip_api/[controller]")]
[ApiController]
public class UserController(IUser user) : ControllerBase
{

    private readonly IUser _userService = user;

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
    /// Retrieves a paginated list of users.
    /// </summary>
    /// <param name="pageNumber">The page number (default is 1).</param>
    /// <param name="pageSize">The number of records per page (default is 20).</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with a paginated list of users.
    /// </returns>
    [HttpGet("Show")]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userService.GetAllAsync(pageNumber, pageSize);
        return Ok(users);
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
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };

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