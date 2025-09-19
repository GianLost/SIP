using SIP.API.Domain.Models.Protocols;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.DTOs.Protocols.Responses;

public class ProtocolResponseDTO : BaseProtocol
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = null;
}