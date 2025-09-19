using SIP.API.Domain.Models.Sectors;

namespace SIP.API.Domain.DTOs.Sectors.Responses;

/// <summary>
/// Represents the data transfer object (DTO) used to return sector information in API responses.
/// Inherits base sector properties and includes creation and update timestamps.
/// </summary>
public class SectorResponseDTO : BaseSector
{
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}