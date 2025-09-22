namespace SIP.API.Domain.DTOs.Sectors.Responses;

/// <summary>
/// Represents the data transfer object (DTO) used to return sector information in API responses.
/// Inherits base sector properties and includes creation and update timestamps.
/// </summary>
public class SectorResponseDTO
{
    public string Name { get; set; } = string.Empty;
    public string Acronym { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}