using SIP.API.Domain.Models.Users;

namespace SIP.API.Domain.DTOs.Users.Responses;

public class UserResponseDTO : BaseUser
{
    public Guid Id { get; set; }

    public bool Status { get; set; }

    public override int Masp { get; set; }
    public override string Name { get; set; } = string.Empty;
    public override string Login { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    public Guid SectorId { get; set; }
}