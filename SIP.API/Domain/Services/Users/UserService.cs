using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
using System.Linq.Expressions;

namespace SIP.API.Domain.Services.Users;

/// <summary>
/// Service implementation for managing user entities in the database.
/// </summary>
public class UserService(ApplicationContext context, EntityCacheManager cache) : IUser
{
    private readonly ApplicationContext _context = context;
    private readonly EntityCacheManager _cache = cache;
    private const string EntityType = nameof(User);

    private const int MaxPageSize = 100;

    /// <inheritdoc/>
    public async Task<User> CreateAsync(UserCreateDTO dto)
    {

        User entity = new()
        {
            FullName = dto.FullName,
            Login = dto.Login,
            Masp = dto.Masp,
            Email = dto.Email,
            PasswordHash = dto?.Password ?? throw new ArgumentNullException(dto!.Password, "password isn't empty."),
            Role = dto.Role,
            SectorId = dto.SectorId
        };

        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();

        ClearTotalUsersCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id) => 
        await _context.Users.Include(p => p.Protocols).FirstOrDefaultAsync(p => p.Id == id);

    /// <inheritdoc/>
    public async Task<ICollection<User>> GetAllAsync() =>
        await _context.Users
            .OrderBy(s => s.FullName)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<UserPagedResultDTO> GetPagedAsync(
    int pageNumber, 
    int pageSize, 
    string? sortLabel, 
    string? sortDirection, 
    string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize); // Limite máximo

        IQueryable<User> query = _context.Users.Include(s => s.Sector).Include(p => p.Protocols);

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Masp.ToString().Contains(searchString) ||
                s.FullName.Contains(searchString) ||
                s.Login.Contains(searchString) ||
                s.Email.Contains(searchString) ||
                (s.Sector != null && s.Sector.Acronym.Contains(searchString)));
        }

        // Get total count (this will use or re-cache based on the token)
        int? totalCount = _cache.Get<int?>($"UserCount_Search_{searchString ?? "NoSearch"}");

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set($"UserCount_Search_{searchString ?? "NoSearch"}", totalCount.Value, EntityType);
        }

        Expression<Func<User, int>> statusOrderExpr = s =>

            s.IsActive == true ? 1 :
            s.IsActive == false ? 0 : 99;

        if (!string.IsNullOrWhiteSpace(sortLabel))
        {
            bool asc = sortDirection?.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? true;

            query = sortLabel.ToLower() switch
            {
                "status" => asc
                    ? query.OrderBy(statusOrderExpr)
                    : query.OrderByDescending(statusOrderExpr),
                "masp" => asc
                    ? query.OrderBy(s => Convert.ToInt64(s.Masp))
                    : query.OrderByDescending(s => Convert.ToInt64(s.Masp)),
                "name" => asc
                    ? query.OrderBy(s => s.FullName)
                    : query.OrderByDescending(s => s.FullName),
                "login" => asc
                    ? query.OrderBy(s => s.Login)
                    : query.OrderByDescending(s => s.Login),
                "sector" => asc
                    ? query.OrderBy(s => s.Sector!.Acronym)
                    : query.OrderByDescending(s => s.Sector!.Acronym),
                _ => asc
                    ? query.OrderBy(statusOrderExpr)
                    : query.OrderByDescending(statusOrderExpr),
            };
        }
        else
        {
            query = query.OrderBy(statusOrderExpr);
        }

        IQueryable<UserListItemDTO> pagedDataQuery = query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserListItemDTO
        {
            Id = u.Id,
            FullName = u.FullName,
            Login = u.Login,
            Masp = u.Masp.ToString(),
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            UpdatedAt = u.UpdatedAt,
            SectorName = u.Sector != null ? u.Sector.Acronym : null,
            Protocols = u.Protocols
        });

        ICollection<UserListItemDTO> items = await pagedDataQuery.ToListAsync();

        return new UserPagedResultDTO
        {
            Items = items,
            TotalCount = totalCount.Value
        };
    }

    /// <inheritdoc/>
    public async Task<User?> UpdateAsync(Guid id, UserUpdateDTO dto)
    {
        User? user = await GetByIdAsync(id);

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

        ClearTotalUsersCountCache();

        return user;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        bool userHasProtocols = await _context.Protocols
        .AnyAsync(p => p.CreatedById == id || p.DestinationUserId == id);

        if (userHasProtocols)
            throw new InvalidOperationException("Não é possível excluir um usuário que possua um ou mais protocolos vinculados.");

        User? user = await GetByIdAsync(id);

        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        ClearTotalUsersCountCache();

        return true;
    }

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
        int? totalCount = _cache.Get<int?>(cacheKey);

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set(cacheKey, totalCount.Value, EntityType);
        }

        return totalCount.Value;
    }

    /// <inheritdoc/>
    public void ClearTotalUsersCountCache() => 
        _cache.Invalidate(EntityType);
}