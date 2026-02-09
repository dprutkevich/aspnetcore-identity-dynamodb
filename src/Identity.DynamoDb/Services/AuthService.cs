using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Models;
using Identity.DynamoDb.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Identity.DynamoDb.Services;

/// <summary>
/// Service for handling authentication operations including login, registration, and token management
/// </summary>
/// <param name="userRepository">Repository for user data operations</param>
/// <param name="tokenRepository">Repository for token management</param>
/// <param name="temporaryTokenRepository">Repository for temporary tokens</param>
/// <param name="notificationService">Optional service for sending notifications</param>
/// <param name="passwordHasher">Service for password hashing and verification</param>
/// <param name="passwordValidator">Service for validating password policy</param>
/// <param name="options">Identity configuration options</param>
public class AuthService(
    IIdentityUserRepository userRepository,
    ITokenRepository tokenRepository,
    ITemporaryTokenRepository temporaryTokenRepository,
    IIdentityNotificationService? notificationService,
    IPasswordHasher passwordHasher,
    IPasswordValidator passwordValidator,
    IOptions<IdentityOptions> options
) : IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's password</param>
    /// <returns>Authentication tokens if successful, error result otherwise</returns>
    public async Task<Result<AuthTokens>> LoginAsync(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail);
        if (user is null)
            return Result.Failure<AuthTokens>(Error.NotFound("User.NotFound", "User not found"));

        if (!passwordHasher.Verify(user.PasswordHash, password))
            return Result.Failure<AuthTokens>(Error.Problem("User.InvalidPassword", "Invalid password"));

        if (!user.IsActive)
            return Result.Failure<AuthTokens>(Error.Problem("User.Inactive", "User is inactive"));

        if (options.Value.RequireEmailConfirmation && !user.IsEmailConfirmed)
            return Result.Failure<AuthTokens>(UserErrors.EmailNotConfirmed);

        var tokens = GenerateTokens(user);
        await tokenRepository.StoreRefreshTokenAsync(
            user.Id,
            tokens.RefreshToken,
            DateTime.UtcNow.AddDays(options.Value.Jwt.RefreshTokenLifetimeDays));

        return Result.Success(tokens);
    }

    /// <summary>
    /// Registers a new user with email and password
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's password</param>
    /// <returns>Authentication tokens if successful, error result otherwise</returns>
    public async Task<Result<AuthTokens>> RegisterAsync(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        var existing = await userRepository.GetByEmailAsync(normalizedEmail);
        if (existing is not null)
            return Result.Failure<AuthTokens>(Error.Conflict("User.AlreadyExists", "User already registered"));

        var (isValidPassword, passwordErrors) = passwordValidator.ValidatePassword(password);
        if (!isValidPassword)
            return Result.Failure<AuthTokens>(Error.Validation("User.InvalidPassword", string.Join("; ", passwordErrors)));

        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(password)
        };

        await userRepository.AddAsync(user);

        var tokens = GenerateTokens(user);
        await tokenRepository.StoreRefreshTokenAsync(
            user.Id,
            tokens.RefreshToken,
            DateTime.UtcNow.AddDays(options.Value.Jwt.RefreshTokenLifetimeDays));

        if (options.Value.SendWelcomeEmail)
        {
            if (notificationService is not null)
            {
                await notificationService.SendWelcomeEmailAsync(user.Id, user.Email);
            }
            else
            {
                Log.Warning("Welcome email not sent: IIdentityNotificationService is not registered");
            }
        }

        return Result.Success(tokens);
    }

    /// <summary>
    /// Changes a user's password after verifying the old password
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="oldPassword">Current password for verification</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>Success result if password changed, error result otherwise</returns>
    public async Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        if (!passwordHasher.Verify(user.PasswordHash, oldPassword))
            return Result.Failure(Error.Problem("User.InvalidPassword", "Invalid password"));

        var (isValidPassword, passwordErrors) = passwordValidator.ValidatePassword(newPassword);
        if (!isValidPassword)
            return Result.Failure(Error.Validation("User.InvalidPassword", string.Join("; ", passwordErrors)));

        user.PasswordHash = passwordHasher.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        if (notificationService is not null)
        {
            await notificationService.SendPasswordChangedAsync(user.Id, user.Email);
        }
        else
        {
            Log.Warning("Password change notification not sent: IIdentityNotificationService is not registered");
        }

        return Result.Success();
    }

    /// <summary>
    /// Generates a new access token using a valid refresh token
    /// </summary>
    /// <param name="refreshToken">Valid refresh token</param>
    /// <returns>New access token if successful, error result otherwise</returns>
    public async Task<Result<string>> RefreshAccessTokenAsync(string refreshToken)
    {
        var isValid = await tokenRepository.IsValidAsync(refreshToken);
        if (!isValid)
            return Result.Failure<string>(Error.Problem("Token.Invalid", "Invalid or expired refresh token"));

        var userId = await tokenRepository.GetUserIdByTokenAsync(refreshToken);
        if (userId is null)
            return Result.Failure<string>(Error.Problem("Token.Invalid", "User not found for token"));

        var user = await userRepository.GetByIdAsync(userId.Value);
        if (user is null)
            return Result.Failure<string>(Error.NotFound("User.NotFound", "User not found"));

        var accessToken = GenerateJwtToken(user.Id, user.Email);
        return Result.Success(accessToken);
    }

    /// <summary>
    /// Sends an email confirmation token to the user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Success result if email sent, error result otherwise</returns>
    public async Task<Result> SendConfirmationEmailAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userId));
        }

        // Invalidate existing email confirmation tokens
        await temporaryTokenRepository.InvalidateUserTokensAsync(userId, TokenType.EmailConfirmation);

        // Create new token
        var token = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var temporaryToken = TemporaryToken.Create(userId, token, TokenType.EmailConfirmation, expiresAt);
        
        await temporaryTokenRepository.AddAsync(temporaryToken);

        if (notificationService is not null)
        {
            await notificationService.SendEmailConfirmationAsync(user.Id, user.Email, token);
        }
        else
        {
            Log.Warning("Confirmation email not sent: IIdentityNotificationService is not registered");
        }

        return Result.Success();
    }

    /// <summary>
    /// Confirms a user's email address using a validation token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="token">Email confirmation token</param>
    /// <returns>Success result if email confirmed, error result otherwise</returns>
    public async Task<Result> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        // Find and validate token
        var temporaryToken = await temporaryTokenRepository.GetByTokenAsync(token, TokenType.EmailConfirmation);
        if (temporaryToken is null || temporaryToken.UserId != userId || !temporaryToken.IsValid)
        {
            return Result.Failure(Error.Problem("Email.InvalidToken", "Invalid or expired token"));
        }

        user.IsEmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        // Mark token as used
        await temporaryTokenRepository.MarkAsUsedAsync(temporaryToken.Id);

        return Result.Success();
    }

    /// <summary>
    /// Generates a password reset token and optionally sends it via notification service
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Success result if token generated, error result otherwise</returns>
    public async Task<Result> GeneratePasswordResetTokenAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        // Invalidate existing password reset tokens
        await temporaryTokenRepository.InvalidateUserTokensAsync(user.Id, TokenType.PasswordReset);

        // Create new token
        var token = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var temporaryToken = TemporaryToken.Create(user.Id, token, TokenType.PasswordReset, expiresAt);
        
        await temporaryTokenRepository.AddAsync(temporaryToken);

        if (notificationService is not null)
        {
            await notificationService.SendPasswordResetAsync(user.Id, user.Email, token);
        }
        else
        {
            Log.Warning("Password reset email not sent: IIdentityNotificationService is not registered");
        }

        return Result.Success();
    }

    /// <summary>
    /// Resets a user's password using a valid reset token
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="token">Password reset token</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>Success result if password reset, error result otherwise</returns>
    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        // Find and validate token
        var temporaryToken = await temporaryTokenRepository.GetByTokenAsync(token, TokenType.PasswordReset);
        if (temporaryToken is null || temporaryToken.UserId != user.Id || !temporaryToken.IsValid)
        {
            return Result.Failure(Error.Problem("Reset.InvalidToken", "Invalid or expired token"));
        }

        var (isValidPassword, passwordErrors) = passwordValidator.ValidatePassword(newPassword);
        if (!isValidPassword)
            return Result.Failure(Error.Validation("User.InvalidPassword", string.Join("; ", passwordErrors)));

        user.PasswordHash = passwordHasher.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        // Mark token as used
        await temporaryTokenRepository.MarkAsUsedAsync(temporaryToken.Id);

        return Result.Success();
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to invalidate</param>
    /// <returns>Success result</returns>
    public async Task<Result> LogoutAsync(string refreshToken)
    {
        await tokenRepository.InvalidateAsync(refreshToken);
        return Result.Success();
    }

    private string GenerateJwtToken(Guid userId, string email)
    {
        var secret = options.Value.Jwt.Secret ?? throw new InvalidOperationException("Jwt secret not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Email, email)
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Value.Jwt.Issuer,
            audience: options.Value.Jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.Value.Jwt.AccessTokenLifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private AuthTokens GenerateTokens(IdentityUser user) => new AuthTokens
    {
        AccessToken = GenerateJwtToken(user.Id, user.Email),
        RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
