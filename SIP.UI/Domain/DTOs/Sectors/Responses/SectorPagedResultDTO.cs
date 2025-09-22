namespace SIP.UI.Domain.DTOs.Sectors.Responses;

/// <summary>
/// DTO for paginated sectors result.
/// </summary>
public class SectorPagedResultDTO
{
    public ICollection<SectorListItemDTO>? Items { get; set; } = [];
    public int TotalCount { get; set; }
}
