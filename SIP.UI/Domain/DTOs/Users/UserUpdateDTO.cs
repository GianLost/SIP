using System.ComponentModel.DataAnnotations;
using SIP.UI.Models.Users;
using SIP.UI.Domain.Enums;

namespace SIP.UI.Domain.DTOs.Users;

public class UserUpdateDTO : BaseUser
{
    public Guid Id { get; set; }

    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    public override RoleEnum Role { get; set; }

    public bool Status { get; set; }
}