using SIP.API.Domain.DTOs.Users.Default;

namespace SIP.API.Domain.DTOs.Sectors.Default;

public class SectorDefaultDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Acronym { get; set; } = string.Empty;
    public ICollection<UserDefaultDTO> Users { get; set; } = [];
}