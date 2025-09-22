using SIP.API.Domain.Interfaces.Sectors;

namespace SIP.API.Domain.DTOs.Sectors;

/// <summary>
/// Represents the data transfer object (DTO) used to create a new sector.
/// Inherits base sector properties.
/// </summary>
public class SectorCreateDTO : ISectorDTO
{
    public string Name { get; set; } = string.Empty;
    public string Acronym { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}