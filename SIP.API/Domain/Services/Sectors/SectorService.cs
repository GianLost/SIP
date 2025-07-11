using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Interfaces.Sectors;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Domain.Services.Sectors;

/// <summary>
/// Service implementation for managing sector entities in the database.
/// </summary>
public class SectorService(ApplicationContext context) : ISector
{
    private readonly ApplicationContext _context = context;

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

        return await query.CountAsync();
    }


    /// <inheritdoc/>
    public async Task<Sector> CreateAsync(SectorCreateDTO dto)
    {
        var sector = new Sector
        {
            Name = dto.Name,
            Acronym = dto.Acronym,
            Phone = dto.Phone
        };

        await _context.Sectors.AddAsync(sector);
        await _context.SaveChangesAsync();

        return sector;
    }

    /// <inheritdoc/>
    public async Task<Sector?> GetByIdAsync(Guid id)
    {
        return await _context.Sectors.Include(s => s.Users).FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <inheritdoc/>
    public async Task<List<Sector>> GetAllAsync(int pageNumber, int pageSize, string? sortLabel, string? sortDirection, string? searchString)
    {
        IQueryable<Sector> query = _context.Sectors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(s =>
                s.Name.Contains(searchString) ||
                s.Acronym.Contains(searchString) ||
                s.Phone.Contains(searchString));
        }

        // Dinamic sorting
        // If sortLabel or sortDirection is provided, apply sorting
        if (!string.IsNullOrWhiteSpace(sortLabel) && !string.IsNullOrWhiteSpace(sortDirection))
        {
            var property = typeof(Sector).GetProperty(sortLabel, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (property != null)
            {
                query = sortDirection.Trim().Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                    ? query.OrderBy(e => EF.Property<object>(e, property.Name))
                    : query.OrderByDescending(e => EF.Property<object>(e, property.Name));
            }
            else
            {
                query = query.OrderBy(s => s.Name); // default sorting if property not found
            }
        }
        else
        {
            query = query.OrderBy(s => s.Name); // default sorting if no sorting parameters are provided
        }

        var result = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return result;
    }

    /// <inheritdoc/>
    public async Task<Sector?> UpdateAsync(Guid id, SectorUpdateDTO dto)
    {
        var sector = await GetByIdAsync(id);

        if (sector == null)
            return null;

        sector.Name = dto.Name;
        sector.Acronym = dto.Acronym;
        sector.Phone = dto.Phone;
        sector.UpdatedAt = DateTime.UtcNow;

        _context.Sectors.Update(sector);
        await _context.SaveChangesAsync();

        return sector;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var sector = await GetByIdAsync(id);

        if (sector == null)
            return false;

        if (sector.Users.Count > 0)
            throw new InvalidOperationException("Não é possível excluir uma secretaria que possua um ou mais usuários vinculados.");

        _context.Sectors.Remove(sector);
        await _context.SaveChangesAsync();

        return true;
    }
}