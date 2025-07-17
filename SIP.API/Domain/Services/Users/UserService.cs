using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Domain.Services.Users;

/// <summary>
/// Service implementation for managing user entities in the database.
/// </summary>
public class UserService(ApplicationContext context, IMemoryCache cache) : IUser
{
    private readonly ApplicationContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private const int MaxPageSize = 100;

    private static CancellationTokenSource _totalUsersCountCacheCts = new();

    /// <inheritdoc/>
    public async Task<int> GetTotalUsersCountAsync(string? searchString)
    {
        IQueryable<User> query = _context.Users;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.FullName.Contains(searchString) ||
                s.Login.Contains(searchString) ||
                s.Email.Contains(searchString));
        }

        string cacheKey = $"UserCount_Search_{searchString ?? "NoSearch"}";

        if (!_cache.TryGetValue(cacheKey, out int totalCount))
        {
            totalCount = await query.CountAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2)) 
                .AddExpirationToken(new CancellationChangeToken(_totalUsersCountCacheCts.Token));

            _cache.Set(cacheKey, totalCount, cacheEntryOptions);
        }

        return totalCount;
    }

    public void ClearTotalUsersCountCache()
    {
        _totalUsersCountCacheCts.Cancel();
        _totalUsersCountCacheCts = new CancellationTokenSource();
    }

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

        ClearTotalUsersCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<List<User>> GetAllAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        IQueryable<User> query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.FullName.Contains(searchString) ||
                s.Login.Contains(searchString) ||
                s.Email.Contains(searchString));
        }

        // Dinamic sorting
        // If sortLabel or sortDirection is provided, apply sorting
        if (!string.IsNullOrWhiteSpace(sortLabel) && !string.IsNullOrWhiteSpace(sortDirection))
        {
            var property = typeof(User).GetProperty(sortLabel, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (property != null)
            {
                query = sortDirection.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                    ? query.OrderBy(e => EF.Property<object>(e, property.Name))
                    : query.OrderByDescending(e => EF.Property<object>(e, property.Name));
            }
            else
            {
                query = query.OrderBy(s => s.FullName); // default sorting if property not found
            }
        }
        else
        {
            query = query.OrderBy(s => s.FullName); // default sorting if no sorting parameters are provided
        }

        var result = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return result;
    }

    /// <inheritdoc/>
    public async Task<UserPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize); // Limite máximo

        IQueryable<User> query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.FullName.Contains(searchString) ||
                s.Login.Contains(searchString) ||
                s.Email.Contains(searchString));
        }

        // Get total count (this will use or re-cache based on the token)
        int totalCount = await GetTotalUsersCountAsync(searchString);

        // Ordenação dinâmica (igual ao seu código atual)
        if (!string.IsNullOrWhiteSpace(sortLabel) && !string.IsNullOrWhiteSpace(sortDirection))
        {
            var property = typeof(User).GetProperty(sortLabel, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (property != null)
            {
                query = sortDirection.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                    ? query.OrderBy(e => EF.Property<object>(e, property.Name))
                    : query.OrderByDescending(e => EF.Property<object>(e, property.Name));
            }
            else
            {
                query = query.OrderBy(s => s.FullName);
            }
        }
        else
        {
            query = query.OrderBy(s => s.FullName);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new UserPagedResultDTO
        {
            Items = items,
            TotalCount = totalCount
        };
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
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
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

        ClearTotalUsersCountCache();

        return true;
    }
}