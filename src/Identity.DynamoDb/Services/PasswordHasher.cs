using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Configuration;
using Microsoft.Extensions.Options;
using BCrypt.Net;

namespace Identity.DynamoDb.Services;

/// <summary>
/// BCrypt-based password hasher implementation
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordOptions _options;

    /// <summary>
    /// Initializes a new instance of the PasswordHasher
    /// </summary>
    /// <param name="options">Password hashing options</param>
    public PasswordHasher(IOptions<IdentityOptions> options)
    {
        _options = options.Value.Password;
    }

    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    /// <param name="password">The plain text password</param>
    /// <returns>The hashed password</returns>
    public string Hash(string password)
    {
        // BCrypt automatically handles salt generation and iterations
        // We use the workFactor (rounds) based on iterations setting
        var workFactor = CalculateWorkFactor(_options.Iterations);
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="hash">The stored hash</param>
    /// <param name="password">The plain text password to verify</param>
    /// <returns>True if the password matches the hash</returns>
    public bool Verify(string hash, string password)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // Return false for any BCrypt exceptions (malformed hash, etc.)
            return false;
        }
    }

    /// <summary>
    /// Calculates BCrypt work factor based on iteration count
    /// BCrypt work factor determines 2^workFactor iterations
    /// </summary>
    /// <param name="iterations">Desired iteration count</param>
    /// <returns>BCrypt work factor</returns>
    private static int CalculateWorkFactor(int iterations)
    {
        // BCrypt uses 2^workFactor iterations
        // Convert our iteration count to appropriate work factor
        // Default to 12 which gives 2^12 = 4096 iterations (good balance of security and performance)
        return iterations switch
        {
            < 4096 => 10,      // 2^10 = 1024
            < 8192 => 11,      // 2^11 = 2048
            < 16384 => 12,     // 2^12 = 4096 (default)
            < 32768 => 13,     // 2^13 = 8192
            < 65536 => 14,     // 2^14 = 16384
            < 131072 => 15,    // 2^15 = 32768
            _ => 16            // 2^16 = 65536 (maximum reasonable)
        };
    }
}
