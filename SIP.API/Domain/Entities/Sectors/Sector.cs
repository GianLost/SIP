using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Models.Sectors;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.Entities.Sectors;

/// <summary>
/// Represents a sector entity in the system.
/// </summary>
[Table("tbl_sectors")]
public class Sector : BaseSector
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyOrder(4)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyOrder(5)]
    public DateTime? UpdatedAt { get; set; } = null;

    [JsonPropertyOrder(6)]
    public ICollection<User> Users { get; set; } = [];
}