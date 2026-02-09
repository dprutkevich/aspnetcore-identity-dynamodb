using Identity.DynamoDb.Models;

namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Repository interface for identity user operations
/// </summary>
public interface IIdentityUserRepository
{
    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <returns>User if found, null otherwise</returns>
    Task<IdentityUser?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves a user by their email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User if found, null otherwise</returns>
    Task<IdentityUser?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Retrieves a user by their username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User if found, null otherwise</returns>
    Task<IdentityUser?> GetByUserNameAsync(string username);
    
    /// <summary>
    /// Adds a new user to the repository
    /// </summary>
    /// <param name="user">User to add</param>
    Task AddAsync(IdentityUser user);
    
    /// <summary>
    /// Updates an existing user in the repository
    /// </summary>
    /// <param name="user">User to update</param>
    Task UpdateAsync(IdentityUser user);
}

