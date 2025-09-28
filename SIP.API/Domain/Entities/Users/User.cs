using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIP.API.Domain.Entities.Users;

/// <summary>
/// Represents a user entity in the system.
/// </summary>
[Table("tbl_users")]
[Index(nameof(Masp), IsUnique = true, Name = "uc_User_Masp")]
[Index(nameof(Name), IsUnique = true, Name = "uc_User_Name")]
[Index(nameof(Login), IsUnique = true, Name = "uc_User_Login")]
[Index(nameof(Email), IsUnique = true, Name = "uc_User_Email")]
[Index(nameof(SectorId), Name = "idx_User_SectorId")]
public class User : BaseUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "int")]
    public override int Masp { get; set; }

    [Required]
    [StringLength(150)]
    [Column(TypeName = "varchar(150)")]
    public override string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public override string Login { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Column(TypeName = "varchar(200)")]
    public override string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public UserRole Role { get; set; }

    [Required]
    [Column(TypeName = "tinyint(1)")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Column(TypeName = "datetime(6)")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "char(36)")]
    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    [Column(TypeName = "datetime(6)")]
    public DateTime? LastLoginAt { get; set; }

    [Column(TypeName = "datetime(6)")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "char(36)")]
    public Guid? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid SectorId { get; set; }
    public Sector? Sector { get; set; }

    public ICollection<Protocol> ProtocolsCreated { get; set; } = [];
    public ICollection<Protocol> ProtocolsReceived { get; set; } = [];
}