using SIP.UI.Domain.Enums;
namespace SIP.UI.Models.Protocols;

public class Protocol
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Number { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ProtocolStatus Status { get; set; }

    public bool IsArchived { get; set; } = false;


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = null;


    public Guid CreatedById { get; set; }
    public Guid DestinationUserId { get; set; }

    public Guid DestinationSectorId { get; set; }
    public Guid OriginSectorId { get; set; }

    //public ICollection<Attachment> Attachments { get; set; } = [];

    //public ICollection<Movement> Movements { get; set; } = [];
}