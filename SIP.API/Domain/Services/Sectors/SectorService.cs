using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Default;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;
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
        Sector entity = new()
        {
            Name = dto.Name,
            Acronym = dto.Acronym,
            Phone = PhoneHelper.ExtractDigits(dto.Phone) // Uso do helper para extrair apenas os dígitos, garantindo o padrão E.164
        };

        await _context.Sectors.AddAsync(entity);
        await _context.SaveChangesAsync();

        ClearTotalSectorsCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<SectorResponseDTO?> GetByIdAsync(Guid id) =>
        await _context.Sectors
        .AsNoTracking()
        .OrderBy(s => s.CreatedAt)
        .Where(s => s.Id == id)
        .Select(s => new SectorResponseDTO
        {
            Id = s.Id,
            Name = s.Name,
            Acronym = s.Acronym,
            Phone = s.Phone,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        })
        .FirstOrDefaultAsync();

    /// <inheritdoc/>
    public async Task<SectorDefaultDTO?> GetByIdDefaultAsync(Guid id) =>
        await _context.Sectors
        .AsNoTracking()
        .OrderBy(s => s.CreatedAt)
        .Where(s => s.Id == id)
        .Select(s => new SectorDefaultDTO
        {
            Id = s.Id,
            Name = s.Name,
            Acronym = s.Acronym,
            Users = s.Users
                .Select(u => new UserDefaultDTO
                {
                    Id = u.Id,
                    Masp = u.Masp,
                    Name = u.Name,
                    Login = u.Login,
                    Email = u.Email,
                    Status = u.IsActive,
                }).ToList()
        })
        .FirstOrDefaultAsync();

    /// <inheritdoc/>
    public async Task<ICollection<SectorDefaultDTO>> GetAllSectorsAsync() =>
    /* TODO: Otimizar consulta para o uso em componente MudSelect no front-end */
    await _context.Sectors
        .AsNoTracking()
        .OrderBy(s => s.CreatedAt)
        .Select(s => new SectorDefaultDTO
        {
            Id = s.Id,
            Name = s.Name,
            Acronym = s.Acronym,
            Phone = s.Phone,
            Users = s.Users
                .Select(u => new UserDefaultDTO
                {
                    Id = u.Id,
                    Masp = u.Masp,
                    Name = u.Name,
                    Login = u.Login,
                    Email = u.Email,
                    Status = u.IsActive
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
                Phone = u.Phone
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

        ClearTotalSectorsCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        SectorDefaultDTO? dto = 
            await GetByIdDefaultAsync(id);

        if (dto == null)
            return false;

        if (dto.Users.Count > 0)
            throw new InvalidOperationException("Não é possível excluir uma secretaria que possua um ou mais usuários vinculados.");

        bool hasProtocols = 
            await _context.Protocols.AnyAsync(p => p.OriginSectorId == id);

        if (hasProtocols)
            throw new InvalidOperationException("Não é possível excluir um setor que possua um ou mais protocolos vinculados.");

        // Agora busca a entidade real para exclusão
        Sector? entity = await _context.Sectors.FindAsync(id);

        if (entity == null)
            return false;

        _context.Sectors.Remove(entity);
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