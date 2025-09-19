using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.DTOs.Protocols;

public class ProtocolCreateDTO : BaseProtocol
{
    [Required]
    public Guid CreatedById { get; set; }

    [Required]
    public Guid OriginSectorId { get; set; }

    [Required]
    public Guid DestinationSectorId { get; set; }

    [Required]
    public Guid DestinationUserId { get; set; }
}