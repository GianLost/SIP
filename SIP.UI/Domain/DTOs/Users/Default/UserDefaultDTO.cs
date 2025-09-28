namespace SIP.UI.Domain.DTOs.Users.Default;

public class UserDefaultDTO
{
    public Guid Id { get; set; }
    public bool Status { get; set; }
    public int Masp { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}