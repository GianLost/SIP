using SIP.API.Domain.DTOs.Users.Default;
using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.DTOs.Sectors.Pagination;
public class SectorListItemDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Acronym { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }

    public ICollection<UserDefaultDTO> Users { get; set; } = [];
}