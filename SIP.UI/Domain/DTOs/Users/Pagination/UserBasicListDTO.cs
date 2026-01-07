namespace SIP.UI.Domain.DTOs.Users.Pagination;

public class UserBasicListDTO
{
    public Guid Id { get; set; }

    public bool Status { get; set; }

    public int Masp { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;

    public string Sector { get; set; } = string.Empty;
}