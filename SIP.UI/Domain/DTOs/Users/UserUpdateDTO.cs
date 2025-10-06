using SIP.UI.Domain.Enums;
using SIP.UI.Domain.Helpers.RegExpressions;
using SIP.UI.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace SIP.UI.Domain.DTOs.Users;

public class UserUpdateDTO : BaseUser
{
    public Guid Id { get; set; }

    public override string Masp { get; set; } = string.Empty;
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;
    public override RoleEnum Role { get; set; }

    /// <summary>
    /// Representação mais amigável para o front-end se o usuário está ativo ou inativo
    /// </summary>
    public bool Status { get; set; }
}