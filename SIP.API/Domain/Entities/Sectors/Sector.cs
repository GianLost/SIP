using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Models.Sectors;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIP.API.Domain.Entities.Sectors;

/// <summary>
/// Represents a sector entity in the system.
/// </summary>
[Table("tbl_sectors")]
[Index(nameof(Name), IsUnique = true, Name = "uc_Sector_Name")]
[Index(nameof(Acronym), IsUnique = true, Name = "uc_Sector_Acronym")]
[Index(nameof(Phone), IsUnique = true, Name = "uc_Sector_Phone")]
public class Sector : BaseSector
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [StringLength(150)]
    [Column(TypeName = "varchar(150)")]
    public override string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public override string Acronym { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public override string Phone { get; set; } = string.Empty;

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

    public ICollection<User> Users { get; set; } = [];
}