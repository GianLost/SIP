using SIP.API.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.DTOs.Sectors.Responses;
public class SectorListItemDTO
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]

    public string Acronym { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }

    public ICollection<User> Users { get; set; } = [];
}