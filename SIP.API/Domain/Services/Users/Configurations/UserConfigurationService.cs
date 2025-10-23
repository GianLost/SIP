using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Hashes.Passwords;
using SIP.API.Domain.Interfaces.Users.Configurations;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Domain.Services.Users.Configurations;

/// <summary>
/// Provides implementations for user configuration operations such as
/// password resets, user-initiated password changes, and sector reassignments.
/// </summary>
/// <remarks>
/// This service isolates user configuration logic from general CRUD management,
/// ensuring a clear separation of administrative and self-service flows.
/// </remarks>
public class UserConfigurationService(ApplicationContext context, ICrypt crypt) : IUserConfiguration
{
    private readonly ApplicationContext _context = context;

    private readonly ICrypt _crypt = crypt;

    /// <inheritdoc/>
    public async Task<User?> ResetPasswordByAdminAsync(Guid id, AdminResetPasswordDTO dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Password))
            return null;

        User? user = 
            await _context.Users.FindAsync(id);

        if (user == null)
            return null;

        user.PasswordHash = _crypt.Hash(dto.Password);

        await UpdateAsync(user);

        return user;
    }

    /// <inheritdoc/>
    public async Task<User?> ChangeOwnPasswordAsync(UserChangePasswordDTO dto)
    {
        if (dto == null ||
            string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
            string.IsNullOrWhiteSpace(dto.NewPassword))
            return null;

        User? user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == dto.Id);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        // Verifica se a senha atual está correta
        bool isValidPassword = _crypt.Verify(dto.CurrentPassword, user.PasswordHash);

        if (!isValidPassword)
            return null;

        // Atualiza para a nova senha
        user.PasswordHash = _crypt.Hash(dto.NewPassword);

        await UpdateAsync(user);

        return user;
    }

    /// <inheritdoc/>
    public async Task<User?> ReassignUserSectorAsync(Guid id, UserChangeSectorDTO dto)
    {
        if (id == Guid.Empty || dto == null || dto.SectorId == Guid.Empty)
            return null;

        User? user = await _context.Users.FindAsync(dto.UserId);

        if (user == null)
            return null;

        bool sectorExists = await _context.Sectors
            .AsNoTracking()
            .AnyAsync(s => s.Id == dto.SectorId);

        if (!sectorExists)
            return null;

        user.SectorId = dto.SectorId;

        await UpdateAsync(user);

        return user;
    }

    /// <summary>
    /// Updates a user entity in the database, setting the last modification timestamp
    /// and persisting changes.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <returns>The updated user entity after persistence.</returns>
    private async Task<User?> UpdateAsync(User user)
    {
        if (user == null)
            return null;

        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return user;
    }
}