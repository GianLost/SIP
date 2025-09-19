using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.DTOs.Protocols;

public class ProtocolCreateDTO : BaseProtocol
{
    [Required]
    [JsonPropertyOrder(9)]
    public Guid CreatedById { get; set; }

    [Required]
    [JsonPropertyOrder(12)]
    public Guid OriginSectorId { get; set; }

    [Required]
    [JsonPropertyOrder(13)]
    public Guid DestinationSectorId { get; set; }

    [Required]
    [JsonPropertyOrder(15)]
    public Guid DestinationUserId { get; set; }
}