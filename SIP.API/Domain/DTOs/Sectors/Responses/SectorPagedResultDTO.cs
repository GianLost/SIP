using SIP.API.Domain.Entities.Sectors;

namespace SIP.API.Domain.DTOs.Sectors.Responses;

/// <summary>
/// DTO for paginated sectors result.
/// </summary>
public class SectorPagedResultDTO
{
    public ICollection<Sector> Items { get; set; } = [];
    public int TotalCount { get; set; }
}