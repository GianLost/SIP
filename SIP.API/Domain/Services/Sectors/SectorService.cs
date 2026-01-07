using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Default.Pagination;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;
using SIP.API.Domain.DTOs.Users.Pagination;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Helpers.KeysHelper;
using SIP.API.Domain.Helpers.PhoneHelper;
using SIP.API.Domain.Interfaces.Sectors;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Domain.Services.Sectors;

/// <summary>
/// Service implementation for managing sector entities in the database.
/// </summary>
public class SectorService(ApplicationContext context, EntityCacheManager cache) : ISector
{
    private readonly ApplicationContext _context = context;
    private readonly EntityCacheManager _cache = cache;

    private const string EntityType = nameof(Sector);
    private const int MaxPageSize = 100;

    /// <inheritdoc/>
    public async Task<Sector> CreateAsync(SectorCreateDTO dto)
    {
        Sector entity = new()
        {
            Name = dto.Name,
            Acronym = dto.Acronym,
            Phone = PhoneHelper.ExtractDigits(dto.Phone) // Uso do helper para extrair apenas os dígitos, garantindo o padrão E.164
        };

        await _context.Sectors.AddAsync(entity);
        await _context.SaveChangesAsync();

        ClearTotalCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<SectorListItemDTO>> GetByIdAsync(Guid id) =>
        await GetByIdTemplateAsync(id, filtered => filtered
        .OrderBy(s => s.CreatedAt)
        .Select(s => new SectorListItemDTO
        {
            Id = s.Id,
            Name = s.Name,
            Acronym = s.Acronym,
            Phone = s.Phone
        }));

    /// <inheritdoc/>
    public async Task<PagedResultDTO<SectorResponseDTO>> GetByIdDefaultAsync(Guid id) =>
        await GetByIdTemplateAsync(id, filtered => filtered
            .OrderBy(s => s.CreatedAt)
            .Select(s => new SectorResponseDTO
            {
                Id = s.Id,
                Name = s.Name,
                Acronym = s.Acronym,
                Phone = s.Phone
            }));

    /// <inheritdoc/>
    public async Task<PagedResultDTO<SectorResponseDTO>> GetAllAsync()
    {
        IQueryable<Sector> query = 
            _context.Sectors.AsNoTracking();

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.SectorsTotalCount, null, null);
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        ICollection<SectorResponseDTO> items = await query
           .OrderBy(s => s.CreatedAt)
           .Select(s => new SectorResponseDTO
           {
               Id = s.Id,
               Name = s.Name,
               Acronym = s.Acronym,
               Phone = s.Phone
           })
           .ToListAsync();

      return new PagedResultDTO<SectorResponseDTO>
      {
          Items = items,
          TotalCount = totalCount
      };

    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<SectorBasicListDTO>> GetSectorsToSelection(
    int skip = 0, 
    int take = 15, 
    string? searchString = null)
    {
        // normalize parameters
        take = Math.Clamp(take, 1, MaxPageSize);
        skip = Math.Max(0, skip);

        IQueryable<Sector> query =
            _context.Sectors.AsNoTracking();

        if (!string.IsNullOrEmpty(searchString))
        {
            string normalized = searchString.Trim().ToLower();
            query = query.Where(s =>
                EF.Functions.Like(s.Acronym.ToLower(), $"%{normalized}%") ||
                EF.Functions.Like(s.Name.ToLower(), $"%{normalized}%"));
        }

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.SectorsTotalCount, searchString, null);
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        // ensure deterministic ordering to make Skip/Take stable across calls
        var items = await query
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Skip(skip)
            .Take(take)
            .Select(s => new SectorBasicListDTO
            {
                Id = s.Id,
                Acronym = s.Acronym,
                Name = s.Name
            })
            .ToListAsync();

        items ??= [];

        return new PagedResultDTO<SectorBasicListDTO>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<SectorListItemDTO>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? sortLabel,
    string? sortDirection,
    string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize);

        IQueryable<Sector> query = 
            _context.Sectors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Name.Contains(searchString) ||
                s.Acronym.Contains(searchString) ||
                s.Phone.Contains(searchString));
        }

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.SectorsTotalCount, searchString, null);
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        // Ordenação
        bool asc = sortDirection?.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? true;
        query = sortLabel?.ToLower() switch
        {
            "name" => asc ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
            "acronym" => asc ? query.OrderBy(s => s.Acronym) : query.OrderByDescending(s => s.Acronym),
            _ => asc ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
        };

        // Paginação + projeção
        ICollection<SectorListItemDTO> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
                .Select(u => new SectorListItemDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Acronym = u.Acronym,
                    Phone = u.Phone
                })
                .ToListAsync();

        return new PagedResultDTO<SectorListItemDTO>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<List<UserBasicListDTO>> GetUsersBySectorAsync(Guid sectorId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.SectorId == sectorId)
            .Select(u => new UserBasicListDTO
            {
                Id = u.Id,
                Status = u.IsActive,
                Masp = u.Masp,
                Name = u.Name,
                Login = u.Login,
                Sector = u.Sector!.Acronym
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Sector?> UpdateAsync(Guid id, SectorUpdateDTO dto)
    {
        Sector? entity = 
            await _context.Sectors.FindAsync(id);

        if (entity == null)
            return null;

        entity.Name = dto.Name;
        entity.Acronym = dto.Acronym;
        entity.Phone = PhoneHelper.ExtractDigits(dto.Phone); // Uso do helper para extrair apenas os dígitos, garantindo o padrão E.164
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Sectors.Update(entity);
        await _context.SaveChangesAsync();

        ClearTotalCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        // 1) Verifica existência simples antes de executar outras checagens
        bool exists = 
            await _context.Sectors
                .AsNoTracking()
                .AnyAsync(s => s.Id == id);

        if (!exists)
            return false;

        // 2) Checa usuários vinculados
        bool hasUsers = 
            await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.SectorId == id);

        if (hasUsers)
            throw new InvalidOperationException("Não é possível excluir uma secretaria que possua um ou mais usuários vinculados.");

        // 3) Checa protocolos vinculados (origem ou destino)
        bool hasProtocols = 
            await _context.Protocols
                .AsNoTracking()
                .AnyAsync(p => p.OriginSectorId == id || p.DestinationSectorId == id);

        if (hasProtocols)
            throw new InvalidOperationException("Não é possível excluir um setor que possua um ou mais protocolos vinculados.");

        // 4) Efetua a exclusão
        try
        {
            int affected = await _context.Sectors
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
            throw new InvalidOperationException("Falha ao excluir o setor devido a restrições no banco de dados.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(string? searchString)
    {
        IQueryable<Sector> query = _context.Sectors;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Name.Contains(searchString) ||
                s.Acronym.Contains(searchString) ||
                s.Phone.Contains(searchString));
        }

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.SectorsTotalCount, searchString, null);
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
    /// A projection function defining how the base <see cref="Sector"/> query should be
    /// transformed into the target DTO type (<typeparamref name="TListDTO"/>).
    /// </param>
    /// <returns>
    /// A <see cref="PagedResultDTO{T}"/> containing the projected entity data and
    /// the total count of matching records.
    /// </returns>
    /// <remarks>
    /// <b>Purpose:</b><br/>
    /// Centralizes and abstracts the repetitive logic used by multiple <c>GetByIdAsync</c>-style methods
    /// within the <see cref="SectorService"/>, ensuring consistency and reusability.
    ///
    /// <b>Behavior:</b><br/>
    /// - Executes a filtered query over <see cref="Sector"/> entities using the provided identifier.<br/>
    /// - Applies the supplied projection expression (<paramref name="projector"/>) to map entities into DTOs.<br/>
    /// - Computes and caches the total count of matching records using <see cref="EntityCacheManager"/>.<br/>
    /// - Returns a <see cref="PagedResultDTO{T}"/> that encapsulates both the results and count, maintaining
    /// uniformity across all service-layer responses.<br/>
    ///
    /// <b>Usage Example:</b><br/>
    /// Used internally by:
    /// <list type="bullet">
    /// <item><see cref="GetByIdAsync(Guid)"/></item>
    /// <item><see cref="GetByIdDefaultAsync(Guid)"/></item>
    /// </list>
    ///
    /// <b>Visibility:</b><br/>
    /// This method is intentionally <c>private</c> to restrict its scope to the <see cref="SectorService"/> implementation,
    /// promoting encapsulation and domain service cohesion.
    /// </remarks>
    private async Task<PagedResultDTO<TListDTO>> GetByIdTemplateAsync<TListDTO>(Guid id, Func<IQueryable<Sector>, IQueryable<TListDTO>> projector)
        where TListDTO : class
    {
        IQueryable<Sector> query =
            _context.Sectors.AsNoTracking();

        IQueryable<Sector> filtered =
            query.Where(u => u.Id == id);

        // chave de cache já considera o contexto (id)
        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.SectorsTotalCount, null, id);

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