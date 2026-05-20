using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Hashes.Passwords;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Domain.Services.Users;

public class AuthenticationService(ICrypt crypt, ApplicationContext context) : IAuthenticationService
{
    private readonly ICrypt _crypt = crypt;
    private readonly ApplicationContext _context = context;

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return null;

        User? user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Login == login);

        if (user == null || !user.IsActive || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        bool validPassword = _crypt.Verify(password, user.PasswordHash);
        if (!validPassword)
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }
}
