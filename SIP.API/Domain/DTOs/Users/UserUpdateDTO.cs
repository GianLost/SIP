using SIP.API.Domain.Enums;
using SIP.API.Domain.Helpers.RegExpressionHelper;
using SIP.API.Domain.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.DTOs.Users;

/// <summary>
/// Represents the data transfer object (DTO) used to update an existing user.
/// Inherits base user properties.
/// </summary>
public class UserUpdateDTO : BaseUser
{
    public Guid Id { get; set; }
    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    public UserRole Role { get; set; }

    /// <summary>
    /// Representação mais amigável para o front-end se o usuário está ativo ou inativo
    /// </summary>
    public bool Status { get; set; }
}