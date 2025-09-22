using SIP.API.Domain.Enums;

namespace SIP.API.Domain.DTOs.Users.Responses;

/// <summary>
/// Represents the data transfer object (DTO) used to return user information in API responses.
/// Inherits base user properties and includes creation and update timestamps.
/// </summary>
public class UserReponseDTO
{
    public Guid Id { get; set; }
    public int Masp { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserRole Role { get; set; }
}