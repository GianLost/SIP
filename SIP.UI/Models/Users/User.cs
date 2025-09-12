using SIP.UI.Domain.Enums;
using SIP.UI.Models.Sectors;
using SIP.UI.Models.Protocols;

namespace SIP.UI.Models.Users;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public int? Masp { get; set; } = null;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public RoleEnum Role { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; } = null;
    public DateTime? UpdatedAt { get; set; } = null;

    public bool ShowProtocols { get; set; }

    public Guid SectorId { get; set; }
    public Sector? Sector { get; set; }

    public ICollection<Protocol> Protocols { get; set; } = [];
}