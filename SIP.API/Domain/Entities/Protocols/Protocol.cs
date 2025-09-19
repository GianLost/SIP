using SIP.API.Domain.Entities.Attachments;
using SIP.API.Domain.Entities.Movements;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIP.API.Domain.Entities.Protocols;

[Table("tbl_protocols")]
[Index(nameof(Number), IsUnique = true, Name = "uc_Protocol_Number")]
[Index(nameof(CreatedById), Name = "idx_Protocol_CreatedById")]
[Index(nameof(DestinationUserId), Name = "idx_Protocol_DestinationUserId")]
[Index(nameof(OriginSectorId), Name = "idx_Protocol_OriginSectorId")]
[Index(nameof(DestinationSectorId), Name = "idx_Protocol_DestinationSectorId")]
public class Protocol : BaseProtocol
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "datetime(6)")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime(6)")]
    public DateTime? UpdatedAt { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid DestinationUserId { get; set; }

    [ForeignKey(nameof(DestinationUserId))]
    public User? DestinationUser { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid OriginSectorId { get; set; }

    [ForeignKey(nameof(OriginSectorId))]
    public Sector? OriginSector { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid DestinationSectorId { get; set; }

    [ForeignKey(nameof(DestinationSectorId))]
    public Sector? DestinationSector { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Movement> Movements { get; set; } = [];
}