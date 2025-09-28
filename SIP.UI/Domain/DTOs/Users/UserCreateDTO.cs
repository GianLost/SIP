using SIP.UI.Domain.DTOs.Sectors.Default;
using SIP.UI.Domain.Enums;
using SIP.UI.Domain.Helpers.RegExpressions;
using SIP.UI.Models.Sectors;
using System.ComponentModel.DataAnnotations;

namespace SIP.UI.Domain.DTOs.Users;

public class UserCreateDTO
{
    [Required(ErrorMessage = "O MASP é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "O MASP deve ser um número positivo.")]
    public string Masp { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    [RegularExpression(ConstExpressions.NameRegex, ErrorMessage = "O nome deve conter apenas letras e espaços.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O login é obrigatório.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O login deve ter entre 3 e 50 caracteres.")]
    [RegularExpression(ConstExpressions.AbridgementRegex, ErrorMessage = "O login deve conter apenas letras, números e os caracteres . _ -")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [StringLength(200, ErrorMessage = "O e-mail deve ter no máximo 200 caracteres.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres.")]
    [RegularExpression(ConstExpressions.StrongPasswordRegex, ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    public RoleEnum Role { get; set; }

    [Required(ErrorMessage = "O setor do usuário é obrigatório.")]
    public Guid SectorId { get; set; }
    public SectorDefaultDTO? Sector { get; set; }
}