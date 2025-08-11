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
    public async Task<ICollection<Protocol>> GetAllAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        IQueryable<Protocol> query = _context.Protocols.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Number.Contains(searchString) ||
                s.Subject.Contains(searchString));
        }

        // Dinamic sorting
        // If sortLabel or sortDirection is provided, apply sorting
        if (!string.IsNullOrWhiteSpace(sortLabel) && !string.IsNullOrWhiteSpace(sortDirection))
        {
            PropertyInfo? property = typeof(Protocol).GetProperty(sortLabel,
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance);

            if (property != null)
            {
                query = sortDirection.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                    ? query.OrderBy(e => EF.Property<object>(e, property.Name))
                    : query.OrderByDescending(e => EF.Property<object>(e, property.Name));
            }
            else
            {
                query = query.OrderBy(s => s.CreatedAt); // default sorting if property not found
            }
        }
        else
        {
            query = query.OrderBy(s => s.CreatedAt); // default sorting if no sorting parameters are provided
        }

        ICollection<Protocol> result = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return result;
    }

    public async Task<ProtocolPagedResultDTO> GetPagedAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize); // Limite máximo

        IQueryable<Protocol> query = _context.Protocols.Include(u => u.CreatedBy).Include(s => s.DestinationSector);

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Number.Contains(searchString) ||
                s.Subject.Contains(searchString));
        }

        // Get total count (this will use or re-cache based on the token)
        int? totalCount = _cache.Get<int?>($"UserCount_Search_{searchString ?? "NoSearch"}");

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set($"UserCount_Search_{searchString ?? "NoSearch"}", totalCount.Value, EntityType);
        }

        if (!string.IsNullOrWhiteSpace(sortLabel) && !string.IsNullOrWhiteSpace(sortDirection))
        {
            PropertyInfo? property = typeof(Protocol).GetProperty(sortLabel,
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance);
            if (property != null)
            {
                query = sortDirection.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                    ? query.OrderBy(e => EF.Property<object>(e, property.Name))
                    : query.OrderByDescending(e => EF.Property<object>(e, property.Name));
            }
            else
            {
                query = query.OrderBy(s => s.CreatedAt);
            }
        }
        else
        {
            query = query.OrderBy(s => s.CreatedAt);
        }

        ICollection<Protocol> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProtocolPagedResultDTO
        {
            Items = items,
            TotalCount = totalCount.Value
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
            throw new InvalidOperationException("Cannot delete an archived protocol.");

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