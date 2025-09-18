using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Enums;

namespace SIP.API.Domain.DTOs.Users.Responses;

public class UserListItemDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? FullName { get; set; } = string.Empty;
    public string? Login { get; set; } = string.Empty;
    public string? Masp { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; } = null;
    public DateTime? UpdatedAt { get; set; } = null;

    public bool ShowProtocols { get; set; }

    public string? SectorName { get; set; }

    public ICollection<Protocol> Protocols { get; set; } = [];
}