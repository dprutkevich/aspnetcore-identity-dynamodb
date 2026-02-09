namespace Identity.DynamoDb.Configuration;

/// <summary>
/// Password policy and hashing configuration options
/// </summary>
public class PasswordOptions
{
    /// <summary>
    /// Minimum password length (default: 8)
    /// </summary>
    public int MinLength { get; set; } = 8;

    /// <summary>
    /// Maximum password length (default: 100)
    /// </summary>
    public int MaxLength { get; set; } = 100;

    /// <summary>
    /// Whether password must contain at least one uppercase letter
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Whether password must contain at least one lowercase letter
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Whether password must contain at least one digit
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Whether password must contain at least one special character
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = true;

    /// <summary>
    /// Number of iterations for password hashing (default: 100,000)
    /// </summary>
    public int Iterations { get; set; } = 100_000;

    /// <summary>
    /// Salt size in bytes for password hashing (default: 16)
    /// </summary>
    public int SaltSize { get; set; } = 16;

    /// <summary>
    /// Hash size in bytes for password hashing (default: 32)
    /// </summary>
    public int HashSize { get; set; } = 32;
}
