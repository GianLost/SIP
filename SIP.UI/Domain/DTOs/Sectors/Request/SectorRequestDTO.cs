using SIP.UI.Domain.DTOs.Users.Request;
using SIP.UI.Models.Sectors;

namespace SIP.UI.Domain.DTOs.Sectors.Request;

/// <summary>
/// Represents the data transfer object (DTO) used to return sector information in API responses.
/// Inherits base sector properties and includes creation and update timestamps.
/// </summary>
public class SectorRequestDTO : BaseSector
{
    public Guid Id { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserDefaultRequestDTO> Users { get; set; } = [];
}