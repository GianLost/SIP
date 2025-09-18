using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;
using System.Text.Json.Serialization;

namespace SIP.API.Domain.DTOs.Users;

/// <summary>
/// Represents the data transfer object (DTO) used to create a new user.
/// Inherits base user properties.
/// </summary>
public class UserCreateDTO : BaseUser
{
    [JsonPropertyOrder(5)]
    public string? Password { get; set; }

    [JsonPropertyOrder(6)]
    public UserRole Role { get; set; }

    [JsonPropertyOrder(8)]
    public Guid SectorId { get; set; }
}