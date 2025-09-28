using SIP.UI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIP.UI.Domain.DTOs.Protocols;

public class ProtocolCreateDTO
{
    public int Number { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; } = false;

    [Required]
    public Guid CreatedById { get; set; }

    [Required]
    public Guid OriginSectorId { get; set; }

    [Required]
    public Guid DestinationSectorId { get; set; }

    [Required]
    public Guid DestinationUserId { get; set; }
}