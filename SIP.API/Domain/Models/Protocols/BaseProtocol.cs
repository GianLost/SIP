using SIP.API.Domain.Enums;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.Models.Protocols;

public class BaseProtocol
{
    [JsonPropertyOrder(1)]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyOrder(3)]
    public ProtocolStatus Status { get; set; }

    [JsonPropertyOrder(4)]
    public bool IsArchived { get; set; } = false;
}