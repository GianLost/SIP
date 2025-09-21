using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users;

/// <summary>
/// Represents the data transfer object (DTO) used to update an existing user.
/// Inherits base user properties.
/// </summary>
public class UserUpdateDTO : BaseUser
{ 
    public UserRole Role { get; set; }

    /// <summary>
    /// Representação mais amigável para o front-end se o usuário está ativo ou inativo
    /// </summary>
    public bool Status { get; set; }
}