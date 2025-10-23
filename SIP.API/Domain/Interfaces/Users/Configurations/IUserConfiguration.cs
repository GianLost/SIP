using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.Interfaces.Users.Configurations;

/// <summary>
/// Defines configuration operations for <see cref="User"/> entities,
/// including password and sector management for both administrative
/// and user self-service contexts.
/// </summary>
/// <remarks>
/// This interface provides specialized methods that allow administrators
/// to reset user passwords or update their sector assignments, and enables
/// users to securely change their own passwords.
/// </remarks>
public interface IUserConfiguration
{
    /// <summary>
    /// Allows an administrator to reset the password of a specific user,
    /// without requiring the current password.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose password will be reset.</param>
    /// <param name="dto">The data transfer object containing the new password and its confirmation.</param>
    /// <returns>
    /// The updated <see cref="User"/> entity if the operation succeeds; otherwise, <see langword="null"/>.
    /// </returns>
    public Task<User?> ResetPasswordByAdminAsync(Guid id, AdminResetPasswordDTO dto);

    /// <summary>
    /// Allows a user to change their own password by providing their current password
    /// along with a new one and its confirmation.
    /// </summary>
    /// <param name="dto">The data transfer object containing the current, new, and confirmed passwords.</param>
    /// <returns>
    /// The updated <see cref="User"/> entity if the operation succeeds and the current password is valid; otherwise, <see langword="null"/>.
    /// </returns>
    public Task<User?> ChangeOwnPasswordAsync(UserChangePasswordDTO dto);

    /// <summary>
    /// Updates the sector assignment of a user, allowing reassignment to a different organizational sector.
    /// </summary>
    /// <param name="id">Unique user identifier to be updated.</param>
    /// <param name="dto">The data transfer object containing the user identifier and the target sector identifier.</param>
    /// <returns>
    /// The updated <see cref="User"/> entity if the operation succeeds; otherwise, <see langword="null"/>.
    /// </returns>
    public Task<User?> ReassignUserSectorAsync(Guid id, UserChangeSectorDTO dto);
}