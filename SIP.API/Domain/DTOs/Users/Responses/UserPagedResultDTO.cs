using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.DTOs.Users.Responses;

/// <summary>
/// DTO for paginated users result.
/// </summary>
public class UserPagedResultDTO
{
    public List<User> Items { get; set; } = [];
    public int TotalCount { get; set; }
}