namespace Identity.DynamoDb.Models;

/// <summary>
/// Represents authentication tokens returned to the client
/// </summary>
public class AuthTokens
{
    /// <summary>
    /// Gets or sets the access token used for API authorization
    /// </summary>
    public required string AccessToken { get; init; }
    
    /// <summary>
    /// Gets or sets the refresh token used to obtain new access tokens
    /// </summary>
    public required string RefreshToken { get; init; }
}

