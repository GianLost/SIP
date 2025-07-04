using SIP.API.Domain.Entities.Attachments;
using SIP.API.Domain.Entities.Movements;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIP.API.Domain.Entities.Protocols;

[Table("tbl_protocols")]
public class Protocol
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;

    public ProtocolStatus Status { get; set; }
    public bool IsArchived { get; set; } = false;

    [ForeignKey(nameof(User))]
    public Guid CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    [ForeignKey(nameof(User))]
    public Guid DestinationSectorId { get; set; }
    public Sector? DestinationSector { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Movement> Movements { get; set; } = [];
}