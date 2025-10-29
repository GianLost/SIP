using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Default.Pagination;
using SIP.API.Domain.DTOs.Users;
using SIP.API.Domain.DTOs.Users.Pagination;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Helpers.KeysHelper;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Hashes.Passwords;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
using System.Linq.Expressions;
using SIP.API.Domain.DTOs.Protocols.Pagination;

namespace SIP.API.Domain.Services.Users;

/// <summary>
/// Service implementation for managing user entities in the database.
/// </summary>
public class UserService(ICrypt cryp, ApplicationContext context, EntityCacheManager cache) : IUser
{
    private readonly ICrypt _crypt = cryp;

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
            PasswordHash = _crypt.Hash(dto.Password!), // Criptografia de senha com Bycrypt
            Role = dto.Role,
            SectorId = dto.SectorId
        };

        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();

        ClearTotalCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<UserBasicListDTO>> GetByIdAsync(Guid id) =>
        await GetByIdTemplateAsync(id, filtered => filtered
        .OrderBy(u => u.CreatedAt)
        .Select(u => new UserBasicListDTO
        {
            Id = u.Id,
            Status = u.IsActive,
            Masp = u.Masp,
            Name = u.Name,
            Login = u.Login,
            Sector = u.Sector!.Acronym
        }));

    /// <inheritdoc/>
    public async Task<PagedResultDTO<UserResponseDTO>> GetByIdDefaultAsync(Guid id) =>
        await GetByIdTemplateAsync(id, filtered => filtered
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserResponseDTO
            {
                Id = u.Id,
                Status = u.IsActive,
                Masp = u.Masp,
                Name = u.Name,
                Login = u.Login,
                Email = u.Email,
                SectorId = u.SectorId
            }));

    /// <inheritdoc/>
    public async Task<PagedResultDTO<UserResponseDTO>> GetAllAsync()
    {
        IQueryable<User> query =
            _context.Users.AsNoTracking();

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.UsersTotalCount, null, null);
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        ICollection<UserResponseDTO> items = await query
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserResponseDTO
            {
                Id = u.Id,
                Status = u.IsActive,
                Masp = u.Masp,
                Name = u.Name,
                Login = u.Login,
                Email = u.Email,
                SectorId = u.SectorId
            }).ToListAsync();

        return new PagedResultDTO<UserResponseDTO>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<UserBasicListDTO>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? sortLabel,
    string? sortDirection,
    string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize);

        IQueryable<User> query =
                _context.Users.AsNoTracking();

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

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.UsersTotalCount, searchString, null);
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        Expression<Func<User, object>> statusOrderExpr = u => u.IsActive ? 0 : 1;

        if (!string.IsNullOrWhiteSpace(sortLabel))
        {
            bool asc = sortDirection?.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? true;

            query = sortLabel.ToLower() switch
            {

                "status" => asc
                  ? query.OrderBy(statusOrderExpr)
                  : query.OrderByDescending(statusOrderExpr),

                "masp" => asc
                  ? query.OrderBy(u => u.Masp)
                  : query.OrderByDescending(s => s.Masp),

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
                  ? query.OrderBy(u => u.CreatedAt)
                  : query.OrderByDescending(u => u.CreatedAt),
            };
        }
        else
        {
            query = query.OrderBy(s => s.CreatedAt);
        }

        IQueryable<UserBasicListDTO> pagedDataQuery = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
                .Select(u => new UserBasicListDTO
                {
                    Id = u.Id,
                    Status = u.IsActive,
                    Masp = u.Masp,
                    Name = u.Name,
                    Login = u.Login,
                    Sector = u.Sector!.Acronym
                });

        ICollection<UserBasicListDTO> items =
            await pagedDataQuery.ToListAsync();

        return new PagedResultDTO<UserBasicListDTO>
        {
            Items = items,
            TotalCount = totalCount
        };

    }

    /// <inheritdoc/>
    public async Task<List<ProtocolBasicListDTO>> GetCreatedProtocolsByUserAsync(Guid userID)
    {
        return await _context.Protocols
            .AsNoTracking()
            .Where(p => p.CreatedById == userID)
            .Select(p => new ProtocolBasicListDTO
            {
                Id = p.Id,
                Status = p.Status,
                Number = p.Number.ToString(),
                Subject = p.Subject,
                CreatedBy = p.CreatedBy!.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<UserListItemDTO>> GetDetailsAsync(Guid id) =>
        await GetByIdTemplateAsync(id, filtered => filtered
            .Select(u => new UserListItemDTO
            {
                Id = u.Id,
                Status = u.IsActive,
                Masp = u.Masp,
                Name = u.Name,
                Login = u.Login,
                Email = u.Email,
                Role = u.Role,
                SectorId = u.SectorId,
                Sector = u.Sector!.Acronym
            }));

    /// <inheritdoc/>
    public async Task<User?> UpdateAsync(Guid id, UserUpdateDTO dto)
    {
        User? user =
            await _context.Users.FindAsync(id);

        if (user == null)
            return null;

        user.Name = dto.Name;
        user.Login = dto.Login;
        user.Masp = dto.Masp;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.IsActive = dto.Status;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        ClearTotalCountCache();

        return user;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        // 1) Verifica existência simples antes de executar outras checagens
        bool exists =
            await _context.Users
                .AsNoTracking()
                .AnyAsync(s => s.Id == id);

        if (!exists)
            return false;

        // 2) Checa protocolos vinculados
        bool hasProtocols =
            await _context.Protocols
                .AsNoTracking()
                .AnyAsync(u => u.CreatedById == id || u.DestinationUserId == id);

        if (hasProtocols)
            throw new InvalidOperationException("Não é possível excluir um usuário que possua um ou mais protocolos vinculados.");

        // 4) Efetua a exclusão
        try
        {
            int affected = await _context.Users
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync();

            if (affected > 0)
            {
                ClearTotalCountCache();
                return true;
            }

            return false;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Falha ao excluir o usuário devido a restrições no banco de dados.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(string? searchString)
    {
        IQueryable<User> query = _context.Users;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Name.Contains(searchString) ||
                s.Login.Contains(searchString) ||
                s.Email.Contains(searchString));
        }

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.UsersTotalCount, searchString, null);
        return await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);
    }

    /// <inheritdoc/>
    public void ClearTotalCountCache() =>
        _cache.Invalidate(EntityType);

  /// <summary>
    /// Provides a generic helper for retrieving entities by their unique identifier,
    /// applying a specified projection and leveraging caching for performance optimization.
    /// </summary>
    /// <typeparam name="TListDTO">
    /// The DTO type representing the projected data structure returned by the query.
    /// </typeparam>
    /// <param name="id">
    /// The unique identifier (<see cref="Guid"/>) of the entity to be retrieved.
    /// </param>
    /// <param name="projector">
    /// A projection function defining how the base <see cref="User"/> query should be
    /// transformed into the target DTO type (<typeparamref name="TListDTO"/>).
    /// </param>
    /// <returns>
    /// A <see cref="PagedResultDTO{T}"/> containing the projected entity data and
    /// the total count of matching records.
    /// </returns>
    /// <remarks>
    /// <b>Purpose:</b><br/>
    /// Centralizes and abstracts the repetitive logic used by multiple <c>GetByIdAsync</c>-style methods
    /// within the <see cref="UserService"/>, ensuring consistency and reusability.
    ///
    /// <b>Behavior:</b><br/>
    /// • Executes a filtered query over <see cref="User"/> entities using the provided identifier.<br/>
    /// • Applies the supplied projection expression (<paramref name="projector"/>) to map entities into DTOs.<br/>
    /// • Computes and caches the total count of matching records using <see cref="EntityCacheManager"/>.<br/>
    /// • Returns a <see cref="PagedResultDTO{T}"/> that encapsulates both the results and count, maintaining
    /// uniformity across all service-layer responses.<br/>
    ///
    /// <b>Usage Example:</b><br/>
    /// Used internally by:
    /// <list type="bullet">
    /// <item><see cref="GetByIdAsync(Guid)"/></item>
    /// <item><see cref="GetByIdDefaultAsync(Guid)"/></item>
    /// <item><see cref="GetDetailsAsync(Guid)"/></item>
    /// </list>
    ///
    /// <b>Visibility:</b><br/>
    /// This method is intentionally <c>private</c> to restrict its scope to the <see cref="UserService"/> implementation,
    /// promoting encapsulation and domain service cohesion.
    /// </remarks>
    private async Task<PagedResultDTO<TListDTO>> GetByIdTemplateAsync<TListDTO>(Guid id, Func<IQueryable<User>, IQueryable<TListDTO>> projector)
        where TListDTO : class
    {
        IQueryable<User> query =
            _context.Users.AsNoTracking();

        IQueryable<User> filtered =
            query.Where(u => u.Id == id);

        // chave de cache já considera o contexto (id)
        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.UsersTotalCount, null, id);

        // conta sobre a query filtrada (e cacheia esse count)
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => filtered.CountAsync(), EntityType);

        // aplica a projeção fornecida e materializa
        ICollection<TListDTO> items =
            await projector(filtered).ToListAsync();

        return new PagedResultDTO<TListDTO>
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}