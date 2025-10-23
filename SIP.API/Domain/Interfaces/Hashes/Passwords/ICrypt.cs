namespace SIP.API.Domain.Interfaces.Hashes.Passwords;

/// <summary>
/// Defines a contract for secure password encryption and verification,
/// providing an abstraction layer over the underlying cryptographic implementation.
/// </summary>
/// <remarks>
/// This interface standardizes password hashing and validation operations,
/// allowing the application to maintain a consistent and testable approach
/// to credential security.
/// </remarks>
public interface ICrypt
{
    /// <summary>
    /// Generates a secure hash for the specified password using a cryptographic algorithm.
    /// </summary>
    /// <param name="password">The plain text password to be hashed.</param>
    /// <returns>
    /// A hashed representation of the password that includes the salt and
    /// work factor used during encryption.
    /// </returns>
    string Hash(string password);

    /// <summary>
    /// Verifies whether the specified plain text password matches the stored password hash.
    /// </summary>
    /// <param name="password">The plain text password provided for verification.</param>
    /// <param name="passwordHash">The previously stored hashed password to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the password matches the hash; otherwise, <see langword="false"/>.
    /// </returns>
    bool Verify(string password, string passwordHash);
}