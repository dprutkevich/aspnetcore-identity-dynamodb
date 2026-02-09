namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Service interface for sending identity-related notifications and emails
/// </summary>
public interface IIdentityNotificationService
{
    /// <summary>
    /// Sends an email confirmation notification
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="email">User's email address</param>
    /// <param name="token">Confirmation token</param>
    Task SendEmailConfirmationAsync(Guid userId, string email, string token);
    
    /// <summary>
    /// Sends a password reset notification
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="email">User's email address</param>
    /// <param name="token">Reset token</param>
    Task SendPasswordResetAsync(Guid userId, string email, string token);
    
    /// <summary>
    /// Sends a password changed notification
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="email">User's email address</param>
    Task SendPasswordChangedAsync(Guid userId, string email);
    
    /// <summary>
    /// Sends a welcome email to new users
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="email">User's email address</param>
    Task SendWelcomeEmailAsync(Guid userId, string email);
}

