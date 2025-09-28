using SIP.UI.Domain.DTOs.Sectors.Default;
using SIP.UI.Domain.Enums;
using SIP.UI.Models.Protocols;

namespace SIP.UI.Domain.DTOs.Users.Pagination;

public class UserListItemDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Masp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public RoleEnum Role { get; set; }

    /// <summary>
    /// Representação mais amigável para o front-end se o usuário está ativo ou inativo
    /// </summary>
    public bool Status { get; set; }
    public bool ShowProtocols { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string SectorName { get; set; } = string.Empty;
    public string SectorAcronym { get; set; } = string.Empty;

    public ICollection<Protocol> Protocols { get; set; } = [];
}