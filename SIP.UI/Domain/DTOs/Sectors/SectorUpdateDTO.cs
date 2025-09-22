using SIP.UI.Domain.DTOs.Interfaces;

namespace SIP.UI.Domain.DTOs.Sectors;

public class SectorUpdateDTO : ISectorDTO
{
    public Guid Id { get; set; } // por exemplo, só no update
    public string Name { get; set; } = string.Empty;
    public string Acronym { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}