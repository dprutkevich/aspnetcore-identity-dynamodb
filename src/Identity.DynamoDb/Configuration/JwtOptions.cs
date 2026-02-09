namespace Identity.DynamoDb.Configuration;

/// <summary>
/// JWT token configuration options
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Secret key for signing JWT tokens. Must be at least 32 characters long
    /// </summary>
    public required string Secret { get; init; }

    /// <summary>
    /// Token issuer (your application name)
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Token audience (your users/clients)
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes (default: 15 minutes)
    /// </summary>
    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    /// <summary>
    /// Refresh token lifetime in days (default: 30 days)
    /// </summary>
    public int RefreshTokenLifetimeDays { get; init; } = 30;
}
