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
            Name = dto.Name,
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
        await _context.Users.Include(p => p.ProtocolsCreated).FirstOrDefaultAsync(p => p.Id == id);

    /// <inheritdoc/>
    public async Task<ICollection<User>> GetAllAsync() =>
        await _context.Users
            .OrderBy(s => s.Name)
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

        IQueryable<User> query = _context.Users
            .Include(s => s.Sector)
            .Include(p => p.ProtocolsCreated)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            if (int.TryParse(searchString, out var maspInt))
            {
                query = query.Where(s =>
                    s.Masp == maspInt ||
                    s.Name.Contains(searchString) ||
                    s.Login.Contains(searchString) ||
                    s.Email.Contains(searchString) ||
                    (s.Sector != null && s.Sector.Acronym.Contains(searchString))
                );
            }
            else
            {
                query = query.Where(s =>
                    s.Name.Contains(searchString) ||
                    s.Login.Contains(searchString) ||
                    s.Email.Contains(searchString) ||
                    (s.Sector != null && s.Sector.Acronym.Contains(searchString))
                );
            }
        }

        // Get total count (this will use or re-cache based on the token)
        int? totalCount = _cache.Get<int?>($"UserCount_Search_{searchString ?? "NoSearch"}");

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set($"UserCount_Search_{searchString ?? "NoSearch"}", totalCount.Value, EntityType);
        }

        Expression<Func<User, object>> statusOrderExpr = u => u.IsActive ? 0 : 1;

        if (!string.IsNullOrWhiteSpace(sortLabel))
        {
            bool asc = sortDirection?.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? true;

            query = sortLabel.ToLower() switch
            {
                "masp" => asc
                  ? query.OrderBy(u => u.Masp)
                  : query.OrderByDescending(s => s.Masp),

                "status" => asc
                  ? query.OrderBy(statusOrderExpr)
                  : query.OrderByDescending(statusOrderExpr),

                "name" => asc
                  ? query.OrderBy(u => u.Name)
                  : query.OrderByDescending(s => s.Name),

                "login" => asc
                  ? query.OrderBy(u => u.Login)
                  : query.OrderByDescending(s => s.Login),

                "sector" => asc
                  ? query.OrderBy(u => u.Sector!.Acronym)
                  : query.OrderByDescending(u => u.Sector!.Acronym),

                _ => asc
                  ? query.OrderBy(u => u.Masp)
                  : query.OrderByDescending(u => u.Masp),
            };
        }
        else
        {
            query = query.OrderBy(s => s.Masp);
        }

        IQueryable<UserListItemDTO> pagedDataQuery = query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserListItemDTO
        {
            Id = u.Id,
            Name = u.Name,
            Login = u.Login,
            Masp = u.Masp.ToString(),
            Email = u.Email,
            Role = u.Role,
            Status = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            UpdatedAt = u.UpdatedAt,
            SectorName = u.Sector!.Acronym,
            Protocols = u.ProtocolsCreated
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

        user.Name = dto.Name;
        user.Login = dto.Login;
        user.Masp = dto.Masp;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.IsActive = dto.Status;
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
                s.Name.Contains(searchString) ||
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