namespace Identity.DynamoDb.Models;

/// <summary>
/// Represents a temporary token for various authentication operations
/// </summary>
public class TemporaryToken
{
    /// <summary>
    /// Gets or sets the unique identifier for the token
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets the user ID this token belongs to
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the token value
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type of the token
    /// </summary>
    public TokenType Type { get; set; }
    
    /// <summary>
    /// Gets or sets when the token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets when the token was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets whether the token has been used
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Creates a new temporary token
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tokenValue">Token value</param>
    /// <param name="type">Token type</param>
    /// <param name="expiresAt">Expiration time</param>
    /// <returns>New temporary token instance</returns>
    public static TemporaryToken Create(Guid userId, string tokenValue, TokenType type, DateTime expiresAt) =>
        new()
        {
            UserId = userId,
            Token = tokenValue,
            Type = type,
            ExpiresAt = expiresAt
        };

    /// <summary>
    /// Checks if the token is valid (not expired and not used)
    /// </summary>
    public bool IsValid => !IsUsed && ExpiresAt > DateTime.UtcNow;
}

/// <summary>
/// Types of temporary tokens
/// </summary>
public enum TokenType
{
    /// <summary>
    /// Token for email confirmation
    /// </summary>
    EmailConfirmation,
    
    /// <summary>
    /// Token for password reset
    /// </summary>
    PasswordReset
}
