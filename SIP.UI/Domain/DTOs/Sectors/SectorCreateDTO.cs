using SIP.UI.Models.Sectors;

namespace SIP.UI.Domain.DTOs.Sectors;

public class SectorCreateDTO : BaseSector
{
    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;
}