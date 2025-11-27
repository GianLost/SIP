using System.ComponentModel.DataAnnotations;
using SIP.UI.Models.Users;
using SIP.UI.Domain.Enums;
using SIP.UI.Domain.Helpers.RegExpressions;

namespace SIP.UI.Domain.DTOs.Users;

public class UserCreateDTO : BaseUser
{
    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres.")]
    [RegularExpression(ConstExpressions.StrongPasswordRegex, ErrorMessage = "A senha deve conter letras M, m, números e caractéres especiais.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    public override RoleEnum Role { get; set; }

    [Required(ErrorMessage = "O setor do usuário é obrigatório.")]
    public Guid SectorId { get; set; }
}