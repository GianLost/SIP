using SIP.API.Domain.Interfaces.Hashes.Passwords;

namespace SIP.API.Domain.Services.Hashes.Passwords;

public class CryptPassword(int workFactor = 12) : ICryptPassword
{
    private readonly int _workFactor = workFactor;

    public string Hash(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: _workFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}