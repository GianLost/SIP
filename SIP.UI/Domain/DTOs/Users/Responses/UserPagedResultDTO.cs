namespace SIP.UI.Domain.DTOs.Users.Responses;

/// <summary>
/// DTO for paginated users result.
/// </summary>
public class UserPagedResultDTO
{
    public ICollection<UserListItemDTO>? Items { get; set; } = [];
    public int TotalCount { get; set; }
}