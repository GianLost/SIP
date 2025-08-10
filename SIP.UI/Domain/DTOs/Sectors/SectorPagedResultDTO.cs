using SIP.UI.Models.Sectors;

namespace SIP.UI.Domain.DTOs.Sectors;

/// <summary>
/// DTO for paginated sectors result.
/// </summary>
public class SectorPagedResultDTO
{
    public ICollection<Sector> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
