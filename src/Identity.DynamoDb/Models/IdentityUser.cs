namespace Identity.DynamoDb.Models;

/// <summary>
/// Represents a user in the identity system
/// </summary>
public class IdentityUser
{
    /// <summary>
    /// Gets or sets the unique identifier for the user
    /// </summary>
    public Guid Id { get; init; }
    
    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's password hash
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Gets or sets the user's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether the user's email has been confirmed
    /// </summary>
    public bool IsEmailConfirmed { get; set; } = false;

    /// <summary>
    /// Gets or sets when the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets when the user account was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
