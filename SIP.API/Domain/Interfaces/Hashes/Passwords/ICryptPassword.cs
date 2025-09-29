namespace SIP.API.Domain.Interfaces.Hashes.Passwords;

public interface ICryptPassword
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}