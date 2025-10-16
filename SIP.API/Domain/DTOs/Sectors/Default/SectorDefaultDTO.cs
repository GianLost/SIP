using SIP.API.Domain.DTOs.Users.Default;
using SIP.API.Domain.Models.Sectors;

namespace SIP.API.Domain.DTOs.Sectors.Default;

public class SectorDefaultDTO : BaseSector
{
    public Guid Id { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;
    public ICollection<UserDefaultDTO> Users { get; set; } = [];
}