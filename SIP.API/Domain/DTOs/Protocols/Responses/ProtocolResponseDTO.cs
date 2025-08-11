using SIP.API.Domain.Models.Protocols;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.DTOs.Protocols.Responses;

public class ProtocolResponseDTO : BaseProtocol
{
    [JsonPropertyOrder(5)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyOrder(6)]
    public DateTime? UpdatedAt { get; set; } = null;
}