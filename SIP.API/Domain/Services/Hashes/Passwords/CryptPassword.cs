using SIP.API.Domain.Interfaces.Hashes.Passwords;

namespace SIP.API.Domain.Services.Hashes.Passwords;

/// <summary>
/// Provides a concrete implementation of <see cref="ICrypt"/> using the BCrypt hashing algorithm
/// for secure password encryption and verification.
/// </summary>
/// <remarks>
/// This service leverages the <c>BCrypt.Net</c> library to generate salted hashes
/// with a configurable work factor, enhancing resistance against brute-force attacks.
/// </remarks>
public class CryptPassword(int workFactor = 12) : ICrypt
{
    private readonly int _workFactor = workFactor;

    /// <summary>
    /// Generates a secure hash for the specified password using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password to be hashed.</param>
    /// <returns>
    /// A hashed representation of the password. Returns an empty string if the provided
    /// password is <see langword="null"/> or empty.
    /// </returns>
    public string Hash(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: _workFactor);
    }

    /// <summary>
    /// Verifies whether the specified password matches the stored BCrypt hash.
    /// </summary>
    /// <param name="password">The plain text password provided for verification.</param>
    /// <param name="passwordHash">The hashed password to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the password matches the hash; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Verify(string password, string passwordHash) => 
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}