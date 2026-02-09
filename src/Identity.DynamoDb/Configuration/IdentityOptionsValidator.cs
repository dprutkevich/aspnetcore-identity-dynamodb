using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Identity.DynamoDb.Configuration;

/// <summary>
/// Validates UMS Identity configuration options
/// </summary>
public class IdentityOptionsValidator : IValidateOptions<IdentityOptions>
{
    /// <summary>
    /// Validates the identity options configuration
    /// </summary>
    /// <param name="name">The name of the options instance</param>
    /// <param name="options">The options to validate</param>
    /// <returns>Validation result indicating success or failure with error messages</returns>
    public ValidateOptionsResult Validate(string? name, IdentityOptions options)
    {
        var failures = new List<string>();

        // Validate JWT configuration
        if (string.IsNullOrEmpty(options.Jwt.Secret))
        {
            failures.Add("JWT Secret is required");
        }
        else if (options.Jwt.Secret.Length < 32)
        {
            failures.Add("JWT Secret must be at least 32 characters long");
        }

        if (options.Jwt.AccessTokenLifetimeMinutes <= 0)
        {
            failures.Add("JWT AccessTokenLifetimeMinutes must be greater than 0");
        }

        if (options.Jwt.RefreshTokenLifetimeDays <= 0)
        {
            failures.Add("JWT RefreshTokenLifetimeDays must be greater than 0");
        }

        // Validate DynamoDB configuration
        if (string.IsNullOrEmpty(options.DynamoDb.UsersTable))
        {
            failures.Add("DynamoDB UsersTable name is required");
        }

        if (string.IsNullOrEmpty(options.DynamoDb.TokensTable))
        {
            failures.Add("DynamoDB TokensTable name is required");
        }

        if (string.IsNullOrEmpty(options.DynamoDb.TemporaryTokensTable))
        {
            failures.Add("DynamoDB TemporaryTokensTable name is required");
        }

        if (string.IsNullOrEmpty(options.DynamoDb.UserRolesTable))
        {
            failures.Add("DynamoDB UserRolesTable name is required");
        }

        // Validate AWS configuration if provided
        if (options.Aws != null)
        {
            if (string.IsNullOrEmpty(options.Aws.Region))
            {
                failures.Add("AWS Region is required when AWS configuration is provided");
            }

            if (options.Aws.UseLocalDynamoDb && string.IsNullOrEmpty(options.Aws.ServiceUrl))
            {
                failures.Add("AWS ServiceUrl is required when UseLocalDynamoDb is true");
            }
        }

        // Validate password options
        if (options.Password.MinLength < 4)
        {
            failures.Add("Password MinLength must be at least 4 characters");
        }

        if (options.Password.MaxLength < options.Password.MinLength)
        {
            failures.Add("Password MaxLength must be greater than or equal to MinLength");
        }

        if (failures.Count > 0)
        {
            return ValidateOptionsResult.Fail(failures);
        }

        return ValidateOptionsResult.Success;
    }
}

