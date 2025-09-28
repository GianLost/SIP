using SIP.API.Domain.Enums;
using SIP.API.Domain.Helpers.RegExpressionHelper;
using SIP.API.Domain.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.DTOs.Users;

/// <summary>
/// Represents the data transfer object (DTO) used to create a new user.
/// Inherits base user properties.
/// </summary>
public class UserCreateDTO : BaseUser
{
    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres.")]
    [RegularExpression(ConstExpressions.StrongPasswordRegex, ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    public UserRole Role { get; set; }

    [Required(ErrorMessage = "O setor do usuário é obrigatório.")]
    public Guid SectorId { get; set; }
}