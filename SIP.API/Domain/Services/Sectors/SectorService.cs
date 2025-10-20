using Microsoft.EntityFrameworkCore;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
using SIP.API.Domain.Interfaces.Sectors;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Default.Pagination;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Helpers.KeysHelper;
using SIP.API.Domain.Helpers.PhoneHelper;

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
    public async Task<PagedResultDTO<SectorResponseDTO>> GetByIdAsync(Guid id)
    {
        IQueryable<Sector> query = 
            _context.Sectors.AsNoTracking();

        string cacheKey = $"{CacheKeys.SectorsTotalCount}";
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        ICollection<SectorResponseDTO> items = await query
            .OrderBy(s => s.CreatedAt)
            .Where(s => s.Id == id)
                .Select(s => new SectorResponseDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Acronym = s.Acronym,
                    Phone = s.Phone,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Users = s.Users
                        .Select(u => new UserDefaultResponseDTO
                        {
                            Id = u.Id,
                            Masp = u.Masp,
                            Name = u.Name,
                            Login = u.Login,
                            Email = u.Email,
                            Status = u.IsActive,
                        }).ToList()
                })
                .ToListAsync();

        return new PagedResultDTO<SectorResponseDTO>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<SectorDefaultResponseDTO>> GetByIdDefaultAsync(Guid id)
    {
        IQueryable<Sector> query = _context.Sectors.AsNoTracking();

        string cacheKey = $"{CacheKeys.SectorsTotalCount}";
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        ICollection<SectorDefaultResponseDTO> items = await query
            .OrderBy(s => s.CreatedAt)
            .Where(s => s.Id == id)
                .Select(s => new SectorDefaultResponseDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Acronym = s.Acronym
                })
                .ToListAsync();

        return new PagedResultDTO<SectorDefaultResponseDTO>
        {
            Items = items,
            TotalCount = totalCount       
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<SectorDefaultResponseDTO>> GetAllAsync()
    {
        /* TODO: Otimizar consulta para o uso em componente MudSelect no front-end */

        IQueryable<Sector> query = _context.Sectors.AsNoTracking();

        string cacheKey = $"{CacheKeys.SectorsTotalCount}";
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        ICollection<SectorDefaultResponseDTO> items = await query
           .OrderBy(s => s.CreatedAt)
           .Select(s => new SectorDefaultResponseDTO
           {
               Id = s.Id,
               Name = s.Name,
               Acronym = s.Acronym,
               Phone = s.Phone
           })
           .ToListAsync();

      return new PagedResultDTO<SectorDefaultResponseDTO>
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

        string cacheKey = $"{CacheKeys.SectorsTotalCount}{searchString ?? "NoSearch"}";
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
                    Phone = u.Phone,
                    Users = u.Users
                        .Select(user => new UserDefaultResponseDTO
                        {
                            Id = user.Id,
                            Masp = user.Masp,
                            Name = user.Name,
                            Login = user.Login,
                            Email = user.Email,
                            Status = user.IsActive,
                        }).ToList()
                })
                .ToListAsync();

        return new PagedResultDTO<SectorListItemDTO>
        {
            Items = items,
            TotalCount = totalCount
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

        string cacheKey = $"{CacheKeys.SectorsTotalCount}{searchString ?? "NoSearch"}";
        return await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);
    }

    /// <inheritdoc/>
    public void ClearTotalCountCache() =>
        _cache.Invalidate(EntityType);
}