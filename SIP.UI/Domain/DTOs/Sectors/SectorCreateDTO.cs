using SIP.UI.Domain.DTOs.Interfaces;

namespace SIP.UI.Domain.DTOs.Sectors;

public class SectorCreateDTO : ISectorDTO
{
    public string Name { get; set; } = string.Empty;
    public string Acronym { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}