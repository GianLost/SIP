using SIP.API.Domain.Models.Users;
using SIP.API.Domain.DTOs.Protocols.Default;
using SIP.API.Domain.Enums;

namespace SIP.API.Domain.DTOs.Users.Responses;

/// <summary>
/// Represents the data transfer object (DTO) used to return user information in API responses.
/// Inherits base user properties and includes creation and update timestamps.
/// </summary>
public class UserResponseDTO : BaseUser
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

    public UserRole Role { get; set; }

    public string SectorAcronym { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProtocolDefaultDTO> ProtocolsCreated { get; set; } = [];
    public ICollection<ProtocolDefaultDTO> ProtocolsReceived { get; set; } = [];
}