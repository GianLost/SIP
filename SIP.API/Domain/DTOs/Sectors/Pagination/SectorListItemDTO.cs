using SIP.API.Domain.Models.Sectors;
using SIP.API.Domain.DTOs.Users.Responses;

namespace SIP.API.Domain.DTOs.Sectors.Pagination;

public class SectorListItemDTO : BaseSector
{
    public Guid Id { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;

    public ICollection<UserDefaultResponseDTO> Users { get; set; } = [];
}