using SIP.UI.Models.Users;

namespace SIP.UI.Domain.DTOs.Users;

public class UserPagedResultDTO
{
    public List<User> Items { get; set; } = [];
    public int TotalCount { get; set; }
}