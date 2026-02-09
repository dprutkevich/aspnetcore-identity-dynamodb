namespace Identity.DynamoDb.Common;

/// <summary>
/// Provides predefined token-related errors for the identity system
/// </summary>
public static class TokenErrors
{
    /// <summary>
    /// Gets an error indicating the token has already been revoked
    /// </summary>
    public static Error TokenAlreadyRevoked => Error.Conflict(
        "Token.AlreadyRevoked",
        "The token has already been revoked");

    /// <summary>
    /// Gets an error indicating the token is invalid
    /// </summary>
    public static Error InvalidToken => Error.Failure(
        "Token.Invalid",
        "The token is invalid");
}


