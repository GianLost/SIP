using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users;

/// <summary>
/// Represents the data transfer object (DTO) used to create a new user.
/// Inherits base user properties.
/// </summary>
public class UserCreateDTO : BaseUser
{
    public string? Password { get; set; }

    public UserRole Role { get; set; }

    public Guid SectorId { get; set; }
}