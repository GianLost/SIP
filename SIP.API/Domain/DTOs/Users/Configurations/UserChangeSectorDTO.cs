namespace SIP.API.Domain.DTOs.Users.Configurations;

public class UserChangeSectorDTO
{
    public Guid UserId { get; set; }
    public Guid SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
}