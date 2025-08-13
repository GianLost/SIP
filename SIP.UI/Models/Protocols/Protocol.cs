using SIP.UI.Domain.Enums;
using SIP.UI.Models.Sectors;
using SIP.UI.Models.Users;
using System.Text.Json.Serialization;

namespace SIP.UI.Models.Protocols;

public class Protocol
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Number { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public ProtocolStatus Status { get; set; }

    public bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyOrder(6)]
    public DateTime? UpdatedAt { get; set; } = null;

    public Guid CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public Guid DestinationSectorId { get; set; }

    public Sector? DestinationSector { get; set; }

    //public ICollection<Attachment> Attachments { get; set; } = [];

    //public ICollection<Movement> Movements { get; set; } = [];
}