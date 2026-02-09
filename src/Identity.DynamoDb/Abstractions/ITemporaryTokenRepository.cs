using Identity.DynamoDb.Models;

namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Repository interface for temporary token operations
/// </summary>
public interface ITemporaryTokenRepository : IRepository<TemporaryToken>
{
    /// <summary>
    /// Retrieves a token by its value and type
    /// </summary>
    /// <param name="token">Token value</param>
    /// <param name="type">Token type</param>
    /// <returns>Token if found, null otherwise</returns>
    Task<TemporaryToken?> GetByTokenAsync(string token, TokenType type);
    
    /// <summary>
    /// Retrieves all tokens for a user by type
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="type">Token type</param>
    /// <returns>List of tokens</returns>
    Task<List<TemporaryToken>> GetByUserIdAsync(Guid userId, TokenType type);
    
    /// <summary>
    /// Marks a token as used
    /// </summary>
    /// <param name="tokenId">Token identifier</param>
    Task MarkAsUsedAsync(Guid tokenId);
    
    /// <summary>
    /// Removes expired tokens from storage
    /// </summary>
    Task CleanupExpiredTokensAsync();
    
    /// <summary>
    /// Invalidates all tokens of a specific type for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="type">Token type to invalidate</param>
    Task InvalidateUserTokensAsync(Guid userId, TokenType type);
}
