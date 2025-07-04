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
    public async Task<IEnumerable<Sector>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _context.Sectors
        .AsNoTracking()
        .Include(s => s.Users)
        .OrderBy(s => s.Name)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
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

        if(sector.Users.Count > 0)
            throw new InvalidOperationException("Cannot delete a sector that has associated users.");

        _context.Sectors.Remove(sector);
        await _context.SaveChangesAsync();

        return true;
    }
}