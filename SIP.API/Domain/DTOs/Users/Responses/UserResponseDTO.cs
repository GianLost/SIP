using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users.Responses;

/// <summary>
/// Represents the data transfer object (DTO) used to return user information in API responses.
/// Inherits base user properties and includes creation and update timestamps.
/// </summary>
public class UserResponseDTO : BaseUser
{
    public Guid Id { get; set; }
    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserRole Role { get; set; }

    public Guid SectorId { get; set; }

    public ICollection<Protocol> ProtocolsCreated { get; set; } = [];
    public ICollection<Protocol> ProtocolsReceived { get; set; } = [];
}