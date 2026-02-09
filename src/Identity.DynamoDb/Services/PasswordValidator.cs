using Identity.DynamoDb.Configuration;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Identity.DynamoDb.Services;

/// <summary>
/// Service for validating passwords according to configured policy
/// </summary>
public interface IPasswordValidator
{
    /// <summary>
    /// Validates a password against the configured policy
    /// </summary>
    /// <param name="password">The password to validate</param>
    /// <returns>Validation result with errors if any</returns>
    (bool IsValid, List<string> Errors) ValidatePassword(string password);
}

/// <summary>
/// Default password validator implementation
/// </summary>
public class PasswordValidator : IPasswordValidator
{
    private readonly PasswordOptions _options;

    /// <summary>
    /// Initializes a new instance of the PasswordValidator
    /// </summary>
    /// <param name="options">Identity options containing password policy</param>
    public PasswordValidator(IOptions<IdentityOptions> options)
    {
        _options = options.Value.Password;
    }

    /// <summary>
    /// Validates a password against the configured policy
    /// </summary>
    /// <param name="password">The password to validate</param>
    /// <returns>Validation result with errors if any</returns>
    public (bool IsValid, List<string> Errors) ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required");
            return (false, errors);
        }

        if (password.Length < _options.MinLength)
        {
            errors.Add($"Password must be at least {_options.MinLength} characters long");
        }

        if (password.Length > _options.MaxLength)
        {
            errors.Add($"Password must not exceed {_options.MaxLength} characters");
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter");
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter");
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit");
        }

        if (_options.RequireSpecialCharacter && !HasSpecialCharacter(password))
        {
            errors.Add("Password must contain at least one special character");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Checks if password contains at least one special character
    /// </summary>
    /// <param name="password">The password to check</param>
    /// <returns>True if password contains special character</returns>
    private static bool HasSpecialCharacter(string password)
    {
        // Common special characters
        var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        return password.Any(specialChars.Contains);
    }
}

