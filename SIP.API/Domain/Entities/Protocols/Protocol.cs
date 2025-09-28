using SIP.API.Domain.Entities.Attachments;
using SIP.API.Domain.Entities.Movements;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.Enums;

namespace SIP.API.Domain.Entities.Protocols;

/// <summary>
/// Represents a protocol entity in the system.
/// </summary>
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
    [Column(TypeName = "int")]
    public override int Number { get; set; }

    [Required]
    [StringLength(200)]
    [Column(TypeName = "varchar(200)")]
    public override string Subject { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "mediumtext")]
    public override string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(50)")]
    public override ProtocolStatus Status { get; set; }

    [Required]
    [Column(TypeName = "tinyint(1)")]
    public override bool IsArchived { get; set; } = false;

    [Required]
    [Column(TypeName = "datetime(6)")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "char(36)")]
    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    [Column(TypeName = "datetime(6)")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "char(36)")]
    public Guid? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid DestinationUserId { get; set; }
    public User? DestinationUser { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid OriginSectorId { get; set; }
    public Sector? OriginSector { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid DestinationSectorId { get; set; }
    public Sector? DestinationSector { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Movement> Movements { get; set; } = [];
}