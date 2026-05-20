using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.Interfaces.Users;

public interface IAuthenticationService
{
    Task<User?> AuthenticateAsync(string login, string password);
    Task<User?> GetUserByIdAsync(Guid userId);
}
