namespace Identity.DynamoDb.Configuration;

/// <summary>
/// Main configuration options for UMS Identity system
/// </summary>
public class IdentityOptions
{
    /// <summary>
    /// Whether to send welcome email after successful registration
    /// </summary>
    public bool SendWelcomeEmail { get; init; } = true;

    /// <summary>
    /// Whether email confirmation is required for new users
    /// </summary>
    public bool RequireEmailConfirmation { get; init; } = true;

    /// <summary>
    /// JWT token configuration
    /// </summary>
    public required JwtOptions Jwt { get; init; }

    /// <summary>
    /// DynamoDB tables configuration
    /// </summary>
    public required DynamoDbTablesOptions DynamoDb { get; init; }

    /// <summary>
    /// AWS configuration for DynamoDB connection
    /// </summary>
    public AwsOptions? Aws { get; init; }

    /// <summary>
    /// Password policy configuration
    /// </summary>
    public PasswordOptions Password { get; init; } = new();
}
