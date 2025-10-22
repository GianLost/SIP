using SIP.API.Domain.Models.Sectors;

namespace SIP.API.Domain.DTOs.Sectors.Responses;

public class SectorResponseDTO : BaseSector
{
    public Guid Id { get; set; }

    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;
}