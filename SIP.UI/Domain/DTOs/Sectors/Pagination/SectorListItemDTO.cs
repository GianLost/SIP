using SIP.UI.Domain.DTOs.Users.Request;
using SIP.UI.Models.Sectors;

namespace SIP.UI.Domain.DTOs.Sectors.Pagination;

public class SectorListItemDTO : BaseSector
{
    public Guid Id { get; set; }

    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;

    public bool ShowUsers { get; set; }

    public ICollection<UserRequestDTO> Users { get; set; } = [];
}