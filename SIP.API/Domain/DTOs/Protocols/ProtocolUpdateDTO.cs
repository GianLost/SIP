using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.DTOs.Protocols;

public class ProtocolUpdateDTO : BaseProtocol
{
    public override int Number { get; set; }
    public override string Subject { get; set; } = string.Empty;
    public override string Description { get; set; } = string.Empty;
    public override ProtocolStatus Status { get; set; }
    public override bool IsArchived { get; set; } = false;

    [Required]
    public Guid CreatedById { get; set; }

    public Guid? UpdatedById { get; set; }

    [Required]
    public Guid OriginSectorId { get; set; }

    [Required]
    public Guid DestinationSectorId { get; set; }

    [Required]
    public Guid DestinationUserId { get; set; }
}