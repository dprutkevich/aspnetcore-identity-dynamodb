namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Repository interface for refresh token operations
/// </summary>
public interface ITokenRepository
{
    /// <summary>
    /// Stores a refresh token for the specified user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="refreshToken">Refresh token value</param>
    /// <param name="expiresAt">Token expiration time</param>
    Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt);
    
    /// <summary>
    /// Checks if a refresh token is valid (exists and not expired/revoked)
    /// </summary>
    /// <param name="refreshToken">Token to validate</param>
    /// <returns>True if token is valid</returns>
    Task<bool> IsValidAsync(string refreshToken);
    
    /// <summary>
    /// Retrieves the user ID associated with a refresh token if the token is valid
    /// </summary>
    /// <param name="refreshToken">Token to look up</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    Task<Guid?> GetUserIdByTokenAsync(string refreshToken);
    
    /// <summary>
    /// Invalidates a refresh token by marking it as revoked
    /// </summary>
    /// <param name="refreshToken">Token to invalidate</param>
    Task InvalidateAsync(string refreshToken);
}

