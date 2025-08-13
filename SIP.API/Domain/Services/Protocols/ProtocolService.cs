using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.DTOs.Protocols.Responses;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Interfaces.Protocols;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
using System.Reflection;

namespace SIP.API.Domain.Services.Protocols;

public class ProtocolService(ApplicationContext contex, EntityCacheManager cache) : IProtocol
{

    private readonly ApplicationContext _context = contex;
    private readonly EntityCacheManager _cache = cache;
    private const string EntityType = nameof(Protocol);
    private const int MaxPageSize = 100;

    /// <inheritdoc/>
    public async Task<Protocol> CreateAsync(ProtocolCreateDTO dto)
    {
        Protocol entity = new()
        {
            Number = dto.Number,
            Subject = dto.Subject,
            Status = dto.Status,
            IsArchived = dto.IsArchived,
            CreatedById = dto.CreatedByID,
            DestinationSectorId = dto.DestinationSectorId
        };

        await _context.Protocols.AddAsync(entity);
        await _context.SaveChangesAsync();

        ClearTotalProtocolsCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<Protocol?> GetByIdAsync(Guid id) =>
        await _context.Protocols.FindAsync(id);

    /// <inheritdoc/>
    public async Task<ICollection<Protocol>> GetAllAsync() =>
        await _context.Protocols
            .OrderBy(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<ProtocolPagedResultDTO> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? sortLabel,
    string? sortDirection,
    string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize);

        IQueryable<Protocol> query = _context.Protocols
            .Include(u => u.CreatedBy)
            .Include(s => s.DestinationSector);

        // Filtro
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var lowered = searchString.ToLower();
            query = query.Where(s =>
                s.Number.ToLower().Contains(lowered) ||
                s.Subject.ToLower().Contains(lowered) ||
                (s.CreatedBy != null && s.CreatedBy.FullName.ToLower().Contains(lowered)) ||
                (s.DestinationSector != null && s.DestinationSector.Acronym.ToLower().Contains(lowered)) ||
                s.Status.ToString().ToLower().Contains(lowered));
        }

        // Ordenação
        sortLabel = sortLabel?.Trim();
        bool asc = sortDirection?.Equals("asc", StringComparison.OrdinalIgnoreCase) ?? true;

        query = sortLabel switch
        {
            "CreatedByName" => asc
                ? query.OrderBy(s => s.CreatedBy!.FullName)
                : query.OrderByDescending(s => s.CreatedBy!.FullName),

            "DestinationSectorAcronym" => asc
                ? query.OrderBy(s => s.DestinationSector!.Acronym)
                : query.OrderByDescending(s => s.DestinationSector!.Acronym),

            _ => asc
                ? query.OrderBy(e => EF.Property<object>(e, sortLabel ?? "CreatedAt"))
                : query.OrderByDescending(e => EF.Property<object>(e, sortLabel ?? "CreatedAt"))
        };

        // Paginação
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new ProtocolPagedResultDTO
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<Protocol?> UpdateAsync(Guid id, ProtocolUpdateDTO dto)
    {
        Protocol? protocol = await GetByIdAsync(id);

        if (protocol == null)
            return null;

        protocol.Number = dto.Number;
        protocol.Subject = dto.Subject;
        protocol.Status = dto.Status;
        protocol.IsArchived = dto.IsArchived;
        protocol.CreatedById = dto.CreatedByID;
        protocol.DestinationSectorId = dto.DestinationSectorId;
        protocol.UpdatedAt = DateTime.UtcNow;

        _context.Protocols.Update(protocol);
        await _context.SaveChangesAsync();

        ClearTotalProtocolsCountCache();

        return protocol;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        Protocol? protocol = await GetByIdAsync(id);

        if (protocol == null)
            return false;

        if (protocol.IsArchived)
            throw new InvalidOperationException("Não é possível excluir um protocolo que está arquivado.");

        _context.Protocols.Remove(protocol);
        await _context.SaveChangesAsync();

        ClearTotalProtocolsCountCache();

        return true;
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalProtocolsCountAsync(string? searchString)
    {
        IQueryable<Protocol> query = _context.Protocols;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Number.Contains(searchString) ||
                s.Subject.Contains(searchString));
        }

        string cacheKey = $"ProtocolCount_Search_{searchString ?? "NoSearch"}";
        int? totalCount = _cache.Get<int?>(cacheKey);

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set(cacheKey, totalCount.Value, EntityType);
        }

        return totalCount.Value;
    }

    /// <inheritdoc/>
    public void ClearTotalProtocolsCountCache() =>
        _cache.Invalidate(EntityType);
}