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

    [Required]
    [JsonPropertyOrder(6)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyOrder(7)]
    public DateTime? UpdatedAt { get; set; } = null;

    [JsonPropertyOrder(8)]
    public string? UpdatedBy { get; set; }

    [Required]
    [JsonPropertyOrder(9)]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    [JsonPropertyOrder(10)]
    public User? CreatedBy { get; set; }

    [Required]
    [JsonPropertyOrder(11)]
    public Guid OriginSectorId { get; set; }

    [ForeignKey(nameof(OriginSectorId))]
    [JsonPropertyOrder(12)]
    public Sector? OriginSector { get; set; }

    [Required]
    [JsonPropertyOrder(13)]
    public Guid DestinationSectorId { get; set; }

    [ForeignKey(nameof(DestinationSectorId))]
    [JsonPropertyOrder(14)]
    public Sector? DestinationSector { get; set; }

    [Required]
    [JsonPropertyOrder(15)]
    public Guid? DestinationUserId { get; set; }

    [ForeignKey(nameof(DestinationUserId))]
    public User? DestinationUser { get; set; }

    [JsonPropertyOrder(16)]
    public ICollection<Attachment> Attachments { get; set; } = [];

    [JsonPropertyOrder(17)]
    public ICollection<Movement> Movements { get; set; } = [];
}