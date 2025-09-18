using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.Entities.Users;

/// <summary>
/// Represents a user entity in the system.
/// </summary>
[Table("tbl_users")]
public class User : BaseUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyOrder(5)]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonPropertyOrder(6)]
    public UserRole Role { get; set; }

    [JsonPropertyOrder(7)]
    public bool IsActive { get; set; } = true;

    [JsonPropertyOrder(8)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyOrder(9)]
    public DateTime? LastLoginAt { get; set; } = null;

    [JsonPropertyOrder(10)]
    public DateTime? UpdatedAt { get; set; } = null;

    [Required]
    [JsonPropertyOrder(11)]
    public Guid SectorId { get; set; }

    [ForeignKey(nameof(SectorId))]
    [JsonPropertyOrder(12)]
    public Sector? Sector { get; set; }

    [JsonPropertyOrder(13)]
    public ICollection<Protocol> Protocols { get; set; } = [];
}