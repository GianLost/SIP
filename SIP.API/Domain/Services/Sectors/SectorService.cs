using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Default;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Users.Default;
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
        Sector sector = new()
        {
            Name = dto.Name,
            Acronym = dto.Acronym,
            Phone = PhoneHelper.ExtractDigits(dto.Phone) // Uso do helper para extrair apenas os dígitos, garantindo o padrão E.164
        };

        await _context.Sectors.AddAsync(sector);
        await _context.SaveChangesAsync();

        ClearTotalSectorsCountCache();

        return sector;
    }

    /// <inheritdoc/>
    public async Task<Sector?> GetByIdAsync(Guid id) =>
        await _context.Sectors
            .OrderBy(s => s.CreatedAt)
            .Include(s => s.Users)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

    /// <inheritdoc/>
    public async Task<ICollection<SectorDefaultDTO>> GetAllSectorsAsync() =>
    /* TODO: Otimizar consulta para o uso em componente MudSelect no front-end */
    await _context.Sectors
        .AsNoTracking()
        .OrderBy(s => s.Name)
        .Select(s => new SectorDefaultDTO
        {
            Id = s.Id,
            Name = s.Name,
            Acronym = s.Acronym,
            Users = s.Users
                .Select(u => new UserDefaultDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Login = u.Login
                }).ToList()
        })
        .ToListAsync();

    /// <inheritdoc/>
    public async Task<SectorPagedResultDTO> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? sortLabel,
    string? sortDirection,
    string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize);

        IQueryable<Sector> query = _context.Sectors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Name.Contains(searchString) ||
                s.Acronym.Contains(searchString) ||
                s.Phone.Contains(searchString));
        }

        // Cache do count
        int? totalCount = _cache.Get<int?>($"{CacheKeys.SectorsTotalCount}{searchString ?? "NoSearch"}");

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set($"{CacheKeys.SectorsTotalCount}{searchString ?? "NoSearch"}", totalCount.Value, EntityType);
        }

        // Ordenação
        bool asc = sortDirection?.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? true;
        query = sortLabel?.ToLower() switch
        {
            "name" => asc ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
            "acronym" => asc ? query.OrderBy(s => s.Acronym) : query.OrderByDescending(s => s.Acronym),
            _ => asc ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
        };

        // Paginação + projeção
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new SectorListItemDTO
            {
                Id = u.Id,
                Name = u.Name,
                Acronym = u.Acronym,
                Phone = u.Phone,
                CreatedAt = u.CreatedAt,
                CreatedById = u.CreatedById,
                UpdatedAt = u.UpdatedAt,
                UpdatedById = u.UpdatedById,
                Users = u.Users.Select(user => new UserDefaultDTO
                {
                    Id = user.Id,
                    Masp = user.Masp,
                    Name = user.Name,
                    Email = user.Email,
                    Login = user.Login,
                    Status = user.IsActive
                }).ToList()
            })
            .ToListAsync();

        return new SectorPagedResultDTO
        {
            Items = items,
            TotalCount = totalCount.Value
        };
    }


    /// <inheritdoc/>
    public async Task<Sector?> UpdateAsync(Guid id, SectorUpdateDTO dto)
    {
        Sector? sector = 
            await GetByIdAsync(id);

        if (sector == null)
            return null;

        sector.Name = dto.Name;
        sector.Acronym = dto.Acronym;
        sector.Phone = PhoneHelper.ExtractDigits(dto.Phone); // Uso do helper para extrair apenas os dígitos, garantindo o padrão E.164
        sector.UpdatedAt = DateTime.UtcNow;

        _context.Sectors.Update(sector);
        await _context.SaveChangesAsync();

        ClearTotalSectorsCountCache();

        return sector;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        Sector? sector = 
            await GetByIdAsync(id);

        if (sector == null)
            return false;

        if (sector.Users.Count > 0)
            throw new InvalidOperationException("Não é possível excluir uma secretaria que possua um ou mais usuários vinculados.");

        _context.Sectors.Remove(sector);
        await _context.SaveChangesAsync();

        ClearTotalSectorsCountCache();

        return true;
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalSectorsCountAsync(string? searchString)
    {
        IQueryable<Sector> query = _context.Sectors;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Name.Contains(searchString) ||
                s.Acronym.Contains(searchString) ||
                s.Phone.Contains(searchString));
        }

        string cacheKey = $"{CacheKeys.SectorsTotalCount}{searchString ?? "NoSearch"}";
        int? totalCount = _cache.Get<int?>(cacheKey);

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set(cacheKey, totalCount.Value, EntityType);
        }

        return totalCount.Value;
    }

    /// <inheritdoc/>
    public void ClearTotalSectorsCountCache() =>
        _cache.Invalidate(EntityType);
    
}