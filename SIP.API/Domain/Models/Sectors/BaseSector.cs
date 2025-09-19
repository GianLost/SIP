using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIP.API.Domain.Models.Sectors;

/// <summary>
/// Provides base properties for sector data transfer objects (DTOs).
/// </summary>
public abstract class BaseSector
{
    [Required]
    [StringLength(150)]
    [Column(TypeName = "varchar(150)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Acronym { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Phone { get; set; } = string.Empty;
}