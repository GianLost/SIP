namespace SIP.API.Domain.DTOs.Sectors;

/// <summary>
/// Represents the data transfer object (DTO) used to create a new sector.
/// Inherits base sector properties.
/// </summary>
public class SectorCreateDTO : BaseSectorDTO 
{ 
    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;
}