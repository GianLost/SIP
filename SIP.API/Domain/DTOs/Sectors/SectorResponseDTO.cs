using SIP.API.Domain.Models.Sectors;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.DTOs.Sectors;

/// <summary>
/// Represents the data transfer object (DTO) used to return sector information in API responses.
/// Inherits base sector properties and includes creation and update timestamps.
/// </summary>
public class SectorResponseDTO : BaseSector
{
    [JsonPropertyOrder(4)]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyOrder(5)]
    public DateTime? UpdatedAt { get; set; }
}