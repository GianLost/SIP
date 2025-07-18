using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users.Responses;

/// <summary>
/// Represents the data transfer object (DTO) used to return user information in API responses.
/// Inherits base user properties and includes creation and update timestamps.
/// </summary>
public class UserReponseDTO : BaseUser
{
    [JsonPropertyOrder(8)]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyOrder(9)]
    public DateTime? UpdatedAt { get; set; }

    public UserRole Role { get; set; }
}