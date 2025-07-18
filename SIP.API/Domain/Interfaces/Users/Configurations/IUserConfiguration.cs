using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.Interfaces.Users.Configurations;

/// <summary>
/// Defines an interface for user configuration and settings operations.
/// </summary>
public interface IUserConfiguration
{
    /// <summary>
    /// Changes a user's password using a default administrative flow.
    /// </summary>
    /// <param name="dto">A data transfer object containing the user's ID and the new password.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the updated User entity if successful; otherwise, returns null if the user is not found.
    /// </returns>
    /// <exception cref="System.ArgumentException">Thrown when the provided password is null or empty.</exception>
    /// <remarks>
    /// This method is intended for administrators or managers and bypasses the need for the user's current password.
    /// </remarks>
    public Task<User?> DefaultChangePasswordAsync(UserDefaultChangePasswordDTO dto);

    public Task<User?> UserChangePasswordAsync(UserChangePasswordDTO dto);

    public Task<User?> UserChangeSectorAsync(UserChangeSectorDTO dto);
}