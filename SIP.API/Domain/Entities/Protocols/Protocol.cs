using SIP.API.Domain.Entities.Attachments;
using SIP.API.Domain.Entities.Movements;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.Entities.Protocols;

[Table("tbl_protocols")]
public class Protocol : BaseProtocol
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyOrder(5)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyOrder(6)]
    public DateTime? UpdatedAt { get; set; } = null;

    [JsonPropertyOrder(7)]
    [ForeignKey(nameof(User))]
    public Guid CreatedById { get; set; }

    [JsonPropertyOrder(8)]
    public User? CreatedBy { get; set; }

    [JsonPropertyOrder(9)]
    [ForeignKey(nameof(Sector))]
    public Guid DestinationSectorId { get; set; }

    [JsonPropertyOrder(10)]
    public Sector? DestinationSector { get; set; }

    [JsonPropertyOrder(11)]
    public ICollection<Attachment> Attachments { get; set; } = [];

    [JsonPropertyOrder(12)]
    public ICollection<Movement> Movements { get; set; } = [];
}