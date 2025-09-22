using SIP.UI.Domain.Helpers.RegExpressions;
using System.ComponentModel.DataAnnotations;

namespace SIP.UI.Domain.DTOs.Users.Configurations;

public class UserDefaultChangePasswordDTO
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres.")]
    [RegularExpression(ConstExpressions.StrongPasswordRegex, ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.")]
    public string Password { get; set; } = string.Empty;
}