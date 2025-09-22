using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Responses;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Interfaces.Sectors;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
using System.Reflection;

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

        string digitsOnly = new([.. dto.Phone.Where(char.IsDigit)]);

        Sector sector = new()
        {
            Name = dto.Name,
            Acronym = dto.Acronym,
            Phone = digitsOnly
        };

        await _context.Sectors.AddAsync(sector);
        await _context.SaveChangesAsync();

        ClearTotalSectorsCountCache();

        return sector;
    }

    /// <inheritdoc/>
    public async Task<Sector?> GetByIdAsync(Guid id) => 
        await _context.Sectors
            .Include(s => s.Users)
            .FirstOrDefaultAsync(s => s.Id == id);

    /// <inheritdoc/>
    public async Task<ICollection<Sector>> GetAllSectorsAsync() => 
        await _context.Sectors
            .OrderBy(s => s.Name)
            .AsNoTracking()
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

        IQueryable<Sector> query = _context.Sectors
            .Include(s => s.Users)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Name.Contains(searchString) ||
                s.Acronym.Contains(searchString) ||
                s.Phone.Contains(searchString));
        }

        // Get total count (this will use or re-cache based on the token)
        int? totalCount = _cache.Get<int?>($"SectorCount_Search_{searchString ?? "NoSearch"}");

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set($"SectorCount_Search_{searchString ?? "NoSearch"}", totalCount.Value, EntityType);
        }

        if (!string.IsNullOrWhiteSpace(sortLabel))
        {
            bool asc = sortDirection?.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? true;

            query = sortLabel.ToLower() switch
            {
                "name" => asc
                  ? query.OrderBy(s => s.Name)
                  : query.OrderByDescending(s => s.Name),

                "acronym" => asc
                  ? query.OrderBy(s => s.Acronym)
                  : query.OrderByDescending(s => s.Acronym),

                _ => asc
                  ? query.OrderBy(s => s.Name)
                  : query.OrderByDescending(s => s.Name),
            };
        }
        else
        {
            query = query.OrderBy(s => s.Name);
        }

        IQueryable<SectorListItemDTO> pagedDataQuery = query
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
            Users = u.Users
        });

        ICollection<SectorListItemDTO> items = await pagedDataQuery.ToListAsync();

        return new SectorPagedResultDTO
        {
            Items = items,
            TotalCount = totalCount.Value
        };
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

        string cacheKey = $"SectorCount_Search_{searchString ?? "NoSearch"}";
        int? totalCount = _cache.Get<int?>(cacheKey);

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set(cacheKey, totalCount.Value, EntityType);
        }

        return totalCount.Value;
    }

    /// <inheritdoc/>
    public async Task<Sector?> UpdateAsync(Guid id, SectorUpdateDTO dto)
    {
        Sector? sector = await GetByIdAsync(id);

        if (sector == null)
            return null;

        sector.Name = dto.Name;
        sector.Acronym = dto.Acronym;
        sector.Phone = dto.Phone;
        sector.UpdatedAt = DateTime.UtcNow;

        _context.Sectors.Update(sector);
        await _context.SaveChangesAsync();

        ClearTotalSectorsCountCache();

        return sector;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        Sector? sector = await GetByIdAsync(id);

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
    public void ClearTotalSectorsCountCache() =>
        _cache.Invalidate(EntityType);
    
}