using SIP.API.Domain.Models.Sectors;

namespace SIP.API.Domain.DTOs.Sectors;

/// <summary>
/// Represents the data transfer object (DTO) used to update an existing sector.
/// Inherits base sector properties.
/// </summary>
public class SectorUpdateDTO : BaseSector
{
    public Guid Id { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Acronym { get; set; } = string.Empty;
    public override string Phone { get; set; } = string.Empty;
}