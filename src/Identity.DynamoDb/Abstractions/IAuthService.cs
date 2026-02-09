using Identity.DynamoDb.Common;
using Identity.DynamoDb.Models;

namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Service interface for authentication and user management operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's password</param>
    /// <returns>Authentication tokens if successful, error result otherwise</returns>
    Task<Result<AuthTokens>> LoginAsync(string email, string password);
    
    /// <summary>
    /// Registers a new user with email and password
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's password</param>
    /// <returns>Authentication tokens if successful, error result otherwise</returns>
    Task<Result<AuthTokens>> RegisterAsync(string email, string password);
    
    /// <summary>
    /// Changes a user's password after verifying the old password
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="oldPassword">Current password for verification</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>Success result if password changed, error result otherwise</returns>
    Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    
    /// <summary>
    /// Generates a new access token using a valid refresh token
    /// </summary>
    /// <param name="refreshToken">Valid refresh token</param>
    /// <returns>New access token if successful, error result otherwise</returns>
    Task<Result<string>> RefreshAccessTokenAsync(string refreshToken);
    
    /// <summary>
    /// Sends an email confirmation token to the user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Success result if email sent, error result otherwise</returns>
    Task<Result> SendConfirmationEmailAsync(Guid userId);
    
    /// <summary>
    /// Confirms a user's email address using a validation token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="token">Email confirmation token</param>
    /// <returns>Success result if email confirmed, error result otherwise</returns>
    Task<Result> ConfirmEmailAsync(Guid userId, string token);
    
    /// <summary>
    /// Generates a password reset token and optionally sends it via notification service
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Success result if token generated, error result otherwise</returns>
    Task<Result> GeneratePasswordResetTokenAsync(string email);
    
    /// <summary>
    /// Resets a user's password using a valid reset token
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="token">Password reset token</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>Success result if password reset, error result otherwise</returns>
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword);
    
    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to invalidate</param>
    /// <returns>Success result</returns>
    Task<Result> LogoutAsync(string refreshToken);
}

