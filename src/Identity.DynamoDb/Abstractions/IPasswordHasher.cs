namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Interface for password hashing and verification operations
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string Hash(string password);
    
    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="hash">Stored password hash</param>
    /// <param name="password">Plain text password to verify</param>
    /// <returns>True if password matches hash</returns>
    bool Verify(string hash, string password);
}

