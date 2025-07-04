using System.Text.Json.Serialization;

namespace SIP.API.Domain.Models.Sectors;

/// <summary>
/// Provides base properties for sector data transfer objects (DTOs).
/// </summary>
public abstract class BaseSector
{
    [JsonPropertyOrder(1)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    public string Acronym { get; set; } = string.Empty;

    [JsonPropertyOrder(3)]
    public string Phone { get; set; } = string.Empty;
}