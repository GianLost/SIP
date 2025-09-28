using SIP.API.Domain.Models.Sectors;

namespace SIP.API.Domain.DTOs.Sectors.Responses;

/// <summary>
/// Represents the data transfer object (DTO) used to return sector information in API responses.
/// Inherits base sector properties and includes creation and update timestamps.
/// </summary>
public class SectorResponseDTO : BaseSector
{
    public Guid Id { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}