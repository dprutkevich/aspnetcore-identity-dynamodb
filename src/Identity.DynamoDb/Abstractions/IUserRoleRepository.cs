using Identity.DynamoDb.Models;

namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Repository interface for user role operations
/// </summary>
public interface IUserRoleRepository : IRepository<UserRole>
{
    /// <summary>
    /// Retrieves all roles for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of user roles</returns>
    Task<List<UserRole>> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if user has the role</returns>
    Task<bool> HasRoleAsync(Guid userId, string roleName);
    
    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="roleName">Role name to assign</param>
    /// <param name="assignedBy">Who assigned the role (optional)</param>
    Task AddRoleAsync(Guid userId, string roleName, Guid? assignedBy = null);
    
    /// <summary>
    /// Removes a role from a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="roleName">Role name to remove</param>
    Task RemoveRoleAsync(Guid userId, string roleName);
    
    /// <summary>
    /// Gets all role names for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of role names</returns>
    Task<List<string>> GetRoleNamesAsync(Guid userId);
}
