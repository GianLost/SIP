using SIP.UI.Domain.DTOs.Sectors.Default;
using SIP.UI.Domain.Enums;
using SIP.UI.Domain.Helpers.RegExpressions;
using SIP.UI.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace SIP.UI.Domain.DTOs.Users;

public class UserCreateDTO : BaseUser
{
    public override string Masp { get; set; } = string.Empty;
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres.")]
    [RegularExpression(ConstExpressions.StrongPasswordRegex, ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.")]
    public string? Password { get; set; }

    public override RoleEnum Role { get; set; }

    [Required(ErrorMessage = "O setor do usuário é obrigatório.")]
    public Guid SectorId { get; set; }
    public SectorDefaultDTO? Sector { get; set; }
}