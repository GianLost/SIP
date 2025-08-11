using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.DTOs.Protocols;

public class ProtocolUpdateDTO : BaseProtocol
{
    [JsonPropertyOrder(7)]
    [ForeignKey(nameof(User))]
    public Guid CreatedByID { get; set; }

    [JsonPropertyOrder(9)]
    [ForeignKey(nameof(Sector))]
    public Guid DestinationSectorId { get; set; }
}