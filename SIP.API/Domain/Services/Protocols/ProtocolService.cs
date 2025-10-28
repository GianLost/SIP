using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Default.Pagination;
using SIP.API.Domain.DTOs.Protocols;
using SIP.API.Domain.DTOs.Protocols.Pagination;
using SIP.API.Domain.DTOs.Protocols.Responses;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Helpers.KeysHelper;
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
        .Where(p => p.Number.ToString().StartsWith(prefix))
        .OrderByDescending(p => p.Number.ToString())
        .Select(p => p.Number.ToString())
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

    public async Task<string> GenerateProtocolNumberAsync()
    {
        string? lastNumber = await GetLastProtocolNumberAsync();
        int nextSequence = GetNextSequence(lastNumber);
        return FormatProtocolNumber(nextSequence);
    }

    /// <inheritdoc/>
    public async Task<Protocol> CreateAsync(ProtocolCreateDTO dto)
    {
        Protocol entity = new()
        {
            Number = Convert.ToInt32(await GenerateProtocolNumberAsync()),
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

        ClearTotalCountCache();

        return entity;
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<ProtocolBasicListDTO>> GetByIdAsync(Guid id) =>
        await GetByIdTemplateAsync(id, filtered => filtered
        .OrderBy(u => u.CreatedAt)
        .Select(u => new ProtocolBasicListDTO
        {
            Id = u.Id,
            Status = u.Status,
            Number = u.Number.ToString(),
            Subject = u.Subject,
            CreatedAt = u.CreatedAt
        }));

    /// <inheritdoc/>
    public async Task<PagedResultDTO<ProtocolResponseDTO>> GetByIdDefaultAsync(Guid id) =>
        await GetByIdTemplateAsync(id, filtered => filtered
            .OrderBy(u => u.CreatedAt)
            .Select(u => new ProtocolResponseDTO
            {
                Id = u.Id,
                Number = u.Number,
                Subject = u.Subject,
                Description = u.Description,
                Status = u.Status,
                IsArchived = u.IsArchived,
                CreatedAt = u.CreatedAt,
                CreatedById = u.CreatedById,
                DestinationUserId = u.DestinationUserId,
                OriginSectorId = u.OriginSectorId,
                DestinationSectorId = u.DestinationSectorId
            }));

    /// <inheritdoc/>
    public async Task<PagedResultDTO<ProtocolResponseDTO>> GetAllAsync()
    {
        IQueryable<Protocol> query =
            _context.Protocols.AsNoTracking();

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.UsersTotalCount, null, null);
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

        ICollection<ProtocolResponseDTO> items = await query
            .OrderBy(u => u.CreatedAt)
            .Select(u => new ProtocolResponseDTO
            {
                Id = u.Id,
                Number = u.Number,
                Subject = u.Subject,
                Description = u.Description,
                Status = u.Status,
                IsArchived = u.IsArchived,
                CreatedAt = u.CreatedAt,
                CreatedById = u.CreatedById,
                DestinationUserId = u.DestinationUserId,
                OriginSectorId = u.OriginSectorId,
                DestinationSectorId = u.DestinationSectorId
            }).ToListAsync();

        return new PagedResultDTO<ProtocolResponseDTO>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResultDTO<ProtocolBasicListDTO>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? sortLabel,
    string? sortDirection,
    string? searchString)
    {
        pageSize = Math.Min(pageSize, MaxPageSize);

        IQueryable<Protocol> query = 
            _context.Protocols.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Number.ToString().Contains(searchString) ||
                s.Subject.Contains(searchString) ||
                (s.CreatedBy != null && s.CreatedBy.Name.Contains(searchString)) ||
                (s.OriginSector != null && s.OriginSector.Acronym.Contains(searchString)) ||
                (s.DestinationUser != null && s.DestinationUser.Name.Contains(searchString)) ||
                (s.DestinationSector != null && s.DestinationSector.Acronym.Contains(searchString)));
        }

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.ProtocolsTotalCount, searchString, null);
        int totalCount = await _cache.GetOrSetCountAsync(cacheKey, () => query.CountAsync(), EntityType);

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
                    ? query.OrderBy(s => s.Number)
                    : query.OrderByDescending(s => s.Number),
                "createdby" => asc
                    ? query.OrderBy(s => s.CreatedBy!.Name)
                    : query.OrderByDescending(s => s.CreatedBy!.Name),
                "originsector" => asc
                    ? query.OrderBy(s => s.OriginSector!.Acronym)
                    : query.OrderByDescending(s => s.OriginSector!.Acronym),
                "destinationto" => asc
                    ? query.OrderBy(s => s.DestinationUser!.Name)
                    : query.OrderByDescending(s => s.DestinationUser!.Name),
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

        IQueryable<ProtocolBasicListDTO> pagedDataQuery = query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProtocolBasicListDTO
        {
            Id = p.Id,
            Status = p.Status,
            Number = p.Number.ToString(),
            Subject = p.Subject,
            CreatedAt = p.CreatedAt
        });


        ICollection<ProtocolBasicListDTO> items = 
            await pagedDataQuery.ToListAsync();

        return new PagedResultDTO<ProtocolBasicListDTO>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<Protocol?> UpdateAsync(Guid id, ProtocolUpdateDTO dto)
    {
        Protocol? protocol =
            await _context.Protocols.FindAsync(id);

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
        protocol.UpdatedById = dto.UpdatedById;

        await _context.SaveChangesAsync();

        ClearTotalCountCache();

        return protocol;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        // 1) Verifica existência simples antes de executar outras checagens
        bool exists =
            await _context.Protocols
                .AsNoTracking()
                .AnyAsync(s => s.Id == id);

        if (!exists)
            return false;

        // 2) Checa se o protocolo está arquivado
        bool isArchived =
            await _context.Protocols
                .AsNoTracking()
                .AnyAsync(p => p.IsArchived);

        if (isArchived)
            throw new InvalidOperationException("Não é possível excluir um protocolo que está arquivado.");

        // 4) Efetua a exclusão
        try
        {
            int affected = await _context.Protocols
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
            throw new InvalidOperationException("Falha ao excluir o protocolo devido a restrições no banco de dados.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(string? searchString)
    {
        IQueryable<Protocol> query = _context.Protocols;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Number.ToString().Contains(searchString) ||
                s.Subject.Contains(searchString));
        }

        string cacheKey = EntityCacheManager.BuildScopedCacheKey(CacheKeys.ProtocolsTotalCount, searchString, null);
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
    /// A projection function defining how the base <see cref="Protocol"/> query should be
    /// transformed into the target DTO type (<typeparamref name="TListDTO"/>).
    /// </param>
    /// <returns>
    /// A <see cref="PagedResultDTO{T}"/> containing the projected entity data and
    /// the total count of matching records.
    /// </returns>
    /// <remarks>
    /// <b>Purpose:</b><br/>
    /// Centralizes and abstracts the repetitive logic used by multiple <c>GetByIdAsync</c>-style methods
    /// within the <see cref="ProtocolService"/>, ensuring consistency and reusability.
    ///
    /// <b>Behavior:</b><br/>
    /// • Executes a filtered query over <see cref="Protocol"/> entities using the provided identifier.<br/>
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
    /// </list>
    ///
    /// <b>Visibility:</b><br/>
    /// This method is intentionally <c>private</c> to restrict its scope to the <see cref="ProtocolService"/> implementation,
    /// promoting encapsulation and domain service cohesion.
    /// </remarks>
    private async Task<PagedResultDTO<TListDTO>> GetByIdTemplateAsync<TListDTO>(Guid id, Func<IQueryable<Protocol>, IQueryable<TListDTO>> projector)
        where TListDTO : class
    {
        IQueryable<Protocol> query =
            _context.Protocols.AsNoTracking();

        IQueryable<Protocol> filtered =
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