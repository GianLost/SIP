using SIP.UI.Models.Sectors;

namespace SIP.UI.Domain.DTOs.Sectors.Request;

public class SectorRequestDTO : BaseSector
{
    public Guid Id { get; set; }

    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;

    public override string ToString() => $"{Acronym} - {Name}";
}