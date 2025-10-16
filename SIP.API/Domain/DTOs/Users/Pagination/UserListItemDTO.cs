using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users.Pagination;

public class UserListItemDTO : BaseUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    /// <summary>
    /// Representação mais amigável para o front-end se o usuário está ativo ou inativo
    /// </summary>
    public bool Status { get; set; }

    public string SectorAcronym { get; set; } = string.Empty;
}