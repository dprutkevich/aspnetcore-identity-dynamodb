namespace Identity.DynamoDb.Configuration;

/// <summary>
/// DynamoDB table names configuration
/// </summary>
public class DynamoDbTablesOptions
{
    /// <summary>
    /// Table name for storing user data
    /// </summary>
    public required string UsersTable { get; init; }

    /// <summary>
    /// Table name for storing refresh tokens
    /// </summary>
    public required string TokensTable { get; init; }
    
    /// <summary>
    /// Table name for storing temporary tokens (email confirmation, password reset)
    /// </summary>
    public required string TemporaryTokensTable { get; init; }
    
    /// <summary>
    /// Table name for storing user roles
    /// </summary>
    public required string UserRolesTable { get; init; }
}

