using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.DTOs.Protocols.Responses;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Interfaces.Protocols;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
using System.Linq.Expressions;

namespace SIP.API.Domain.Services.Protocols;

public class ProtocolService(ApplicationContext contex, EntityCacheManager cache) : IProtocol
{

    private readonly ApplicationContext _context = contex;
    private readonly EntityCacheManager _cache = cache;
    private const string EntityType = nameof(Protocol);
    private const int MaxPageSize = 100;

    /// <inheritdoc/>
    public async Task<string?> GetLastProtocolNumberAsync()
    {
        int year = DateTime.UtcNow.Year;
        string prefix = year.ToString();

        // Busca o último número gerado para o ano atual no banco
        return await _context.Protocols
        .Where(p => p.Number.StartsWith(prefix))
        .OrderByDescending(p => p.Number)
        .Select(p => p.Number)
        .FirstOrDefaultAsync();
    }

    public int GetNextSequence(string? lastProtocolNumber)
    {
        int year = DateTime.UtcNow.Year;
        string prefix = year.ToString();
        int nextSequence = 1;

        if (lastProtocolNumber != null)
        {
            // Extrai a parte sequencial
            string lastSequencePart = lastProtocolNumber[prefix.Length..];
            if (int.TryParse(lastSequencePart, out int lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }
        return nextSequence;
    }

    public string FormatProtocolNumber(int nextSequence)
    {
        int year = DateTime.UtcNow.Year;
        string prefix = year.ToString();
        return $"{prefix}{nextSequence:D5}";
    }

    // MANTENHA ESTE MÉTODO PARA USO EM OUTROS LUGARES, COMO NA CRIAÇÃO DE UM ÚNICO PROTOCOLO
    public async Task<string> GenerateProtocolNumberAsync()
    {
        var lastNumber = await GetLastProtocolNumberAsync();
        var nextSequence = GetNextSequence(lastNumber);
        return FormatProtocolNumber(nextSequence);
    }

    /// <inheritdoc/>
    public async Task<Protocol> CreateAsync(ProtocolCreateDTO dto)
    {
        Protocol entity = new()
        {
            Number = await GenerateProtocolNumberAsync(),
            Subject = dto.Subject,
            Description = dto.Description,
            Status = dto.Status,
            IsArchived = dto.IsArchived,
            CreatedById = dto.CreatedById,
            OriginSectorId = dto.OriginSectorId,
            DestinationSectorId = dto.DestinationSectorId,
            DestinationUserId = dto.DestinationUserId
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

        IQueryable<Protocol> query = _context.Protocols.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Number.Contains(searchString) ||
                s.Subject.Contains(searchString) ||
                (s.CreatedBy != null && s.CreatedBy.FullName.Contains(searchString)) ||
                (s.OriginSector != null && s.OriginSector.Acronym.Contains(searchString)) ||
                (s.DestinationUser != null && s.DestinationUser.FullName.Contains(searchString)) ||
                (s.DestinationSector != null && s.DestinationSector.Acronym.Contains(searchString)));
        }

        int? totalCount = _cache.Get<int?>($"ProtocolCount_Search_{searchString ?? "NoSearch"}");

        if (!totalCount.HasValue)
        {
            totalCount = await query.CountAsync();
            _cache.Set($"ProtocolCount_Search_{searchString ?? "NoSearch"}", totalCount.Value, EntityType);
        }

        Expression<Func<Protocol, int>> statusOrderExpr = s =>
            s.Status == ProtocolStatus.Open ? 1 :
            s.Status == ProtocolStatus.SentForReview ? 2 :
            s.Status == ProtocolStatus.Received ? 3 :
            s.Status == ProtocolStatus.UnderReview ? 4 :
            s.Status == ProtocolStatus.CorrectionRequested ? 5 :
            s.Status == ProtocolStatus.Approved ? 6 :
            s.Status == ProtocolStatus.Rejected ? 7 :
            s.Status == ProtocolStatus.Finalized ? 8 : 99;

        if (!string.IsNullOrWhiteSpace(sortLabel))
        {
            bool asc = sortDirection?.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? true;

            query = sortLabel.ToLower() switch
            {
                "status" => asc
                    ? query.OrderBy(statusOrderExpr)
                    : query.OrderByDescending(statusOrderExpr),
                "number" => asc
                    ? query.OrderBy(s => Convert.ToInt64(s.Number))
                    : query.OrderByDescending(s => Convert.ToInt64(s.Number)),
                "createdby" => asc
                    ? query.OrderBy(s => s.CreatedBy!.FullName)
                    : query.OrderByDescending(s => s.CreatedBy!.FullName),
                "originsector" => asc
                    ? query.OrderBy(s => s.OriginSector!.Acronym)
                    : query.OrderByDescending(s => s.OriginSector!.Acronym),
                "destinationto" => asc
                    ? query.OrderBy(s => s.DestinationUser!.FullName)
                    : query.OrderByDescending(s => s.DestinationUser!.FullName),
                "destinationsector" => asc
                    ? query.OrderBy(s => s.DestinationSector!.Acronym)
                    : query.OrderByDescending(s => s.DestinationSector!.Acronym),
                _ => asc
                    ? query.OrderBy(statusOrderExpr)
                    : query.OrderByDescending(statusOrderExpr),
            };
        }
        else
        {
            query = query.OrderBy(statusOrderExpr);
        }

        IQueryable<ProtocolListItemDto> pagedDataQuery = query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProtocolListItemDto
        {
            Id = p.Id,
            Number = p.Number,
            Subject = p.Subject,
            Description = p.Description,
            Status = p.Status,
            IsArchived = p.IsArchived,
            CreatedByFullName = p.CreatedBy != null ? p.CreatedBy.FullName : null,
            OriginSectorAcronym = p.OriginSector != null ? p.OriginSector.Acronym : null,
            DestinationUserFullName = p.DestinationUser != null ? p.DestinationUser.FullName : null,
            DestinationSectorAcronym = p.DestinationSector != null ? p.DestinationSector.Acronym : null
        });


        ICollection<ProtocolListItemDto> items = await pagedDataQuery.ToListAsync();

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

        protocol.Subject = dto.Subject;
        protocol.Description = dto.Description;
        protocol.Status = dto.Status;
        protocol.IsArchived = dto.IsArchived;
        protocol.CreatedById = dto.CreatedById;
        protocol.DestinationUserId = dto.DestinationUserId;
        protocol.DestinationSectorId = dto.DestinationSectorId;
        protocol.OriginSectorId = dto.OriginSectorId;
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