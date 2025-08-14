using SIP.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.Models.Protocols;

public class BaseProtocol
{
    [Required]
    [JsonPropertyOrder(1)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [JsonPropertyOrder(2)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    [JsonPropertyOrder(3)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [JsonPropertyOrder(4)]
    public ProtocolStatus Status { get; set; }

    [JsonPropertyOrder(5)]
    public bool IsArchived { get; set; } = false;
}