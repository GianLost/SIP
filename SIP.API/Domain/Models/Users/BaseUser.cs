using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIP.API.Domain.Models.Users;

/// <summary>
/// Provides base properties for user data transfer objects (DTOs).
/// </summary>
public class BaseUser
{
    [Required]
    [StringLength(150)]
    [Column(TypeName = "varchar(150)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string Login { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "int")]
    public int Masp { get; set; }

    [Required]
    [StringLength(200)]
    [Column(TypeName = "varchar(200)")]
    public string Email { get; set; } = string.Empty;
}