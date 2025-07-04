using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Domain.Services.Users;

/// <summary>
/// Service implementation for managing user entities in the database.
/// </summary>
public class UserService(ApplicationContext context) : IUser
{
    private readonly ApplicationContext _context = context;

    /// <inheritdoc/>
    public async Task<User> CreateAsync(UserCreateDTO dto)
    {

        var entity = new User
        {
            FullName = dto.FullName,
            Login = dto.Login,
            Masp = dto.Masp,
            Email = dto.Email,
            PasswordHash = dto?.PasswordHash ?? throw new ArgumentNullException("password isn't empty."),
            Role = dto.Role,
            SectorId = dto.SectorId
        };

        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _context.Users
        .AsNoTracking()
        .OrderBy(u => u.FullName)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<User?> UpdateAsync(Guid id, UserUpdateDTO dto)
    {
        var user = await GetByIdAsync(id);

        if (user == null)
            return null;

        user.FullName = dto.FullName;
        user.Login = dto.Login;
        user.Masp = dto.Masp;
        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);

        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return false;

        if (user.Protocols.Count > 0)
            throw new InvalidOperationException("Cannot delete user with associated protocols.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }
}