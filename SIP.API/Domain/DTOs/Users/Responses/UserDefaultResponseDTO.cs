using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users.Responses;

public class UserDefaultResponseDTO : BaseUser
{
    public Guid Id { get; set; }

    /// <summary>
    /// Representação mais amigável para o front-end se o usuário está ativo ou inativo
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;
}