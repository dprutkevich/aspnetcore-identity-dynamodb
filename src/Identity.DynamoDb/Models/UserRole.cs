namespace Identity.DynamoDb.Models;

/// <summary>
/// Represents a role assigned to a user
/// </summary>
public class UserRole
{
    /// <summary>
    /// Gets or sets the unique identifier for the user role assignment
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets the user ID this role is assigned to
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the role
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets when the role was assigned
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets who assigned this role (optional)
    /// </summary>
    public Guid? AssignedBy { get; set; }

    /// <summary>
    /// Creates a new user role assignment
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Role name</param>
    /// <param name="assignedBy">Who assigned the role (optional)</param>
    /// <returns>New user role instance</returns>
    public static UserRole Create(Guid userId, string roleName, Guid? assignedBy = null) =>
        new()
        {
            UserId = userId,
            RoleName = roleName,
            AssignedBy = assignedBy
        };
}
