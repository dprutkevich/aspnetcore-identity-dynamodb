using System.Runtime.InteropServices;

namespace Identity.DynamoDb.Models;

/// <summary>
/// Represents a refresh token for JWT authentication
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Gets or sets the unique identifier for the refresh token
    /// </summary>
    public Guid Id { get; init; }
    
    /// <summary>
    /// Gets or sets the user ID this token belongs to
    /// </summary>
    public Guid UserId { get; init; }
    
    /// <summary>
    /// Gets or sets the token string
    /// </summary>
    public string Token { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or sets when the token expires
    /// </summary>
    public DateTime ExpiresAt { get; init; }
    
    /// <summary>
    /// Gets or sets whether the token has been revoked
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Creates a new refresh token with the specified parameters
    /// </summary>
    /// <param name="userId">The user ID this token belongs to</param>
    /// <param name="token">The token string</param>
    /// <param name="expiresAt">When the token expires</param>
    /// <returns>A new refresh token instance</returns>
    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt
        };
}

