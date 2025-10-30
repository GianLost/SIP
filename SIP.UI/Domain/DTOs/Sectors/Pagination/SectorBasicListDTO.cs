namespace SIP.UI.Domain.DTOs.Sectors.Pagination;

public class SectorBasicListDTO
{
    public Guid Id { get; set; }
    public string Acronym { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}