using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Models;
using Identity.DynamoDb.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace Identity.DynamoDb.Repositories;

/// <summary>
/// Repository for managing refresh tokens in DynamoDB
/// </summary>
/// <param name="dynamoDbClient">DynamoDB client instance</param>
/// <param name="options">Identity configuration options</param>
public class TokenRepository(IAmazonDynamoDB dynamoDbClient, IOptions<IdentityOptions> options) : 
    Repository<RefreshToken>(dynamoDbClient, options.Value.DynamoDb.TokensTable), ITokenRepository
{
    /// <summary>
    /// Stores a refresh token for the specified user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="refreshToken">Refresh token value</param>
    /// <param name="expiresAt">Token expiration time</param>
    public async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt) => 
        await AddAsync(RefreshToken.Create(userId, refreshToken, expiresAt));

    /// <summary>
    /// Checks if a refresh token is valid (exists and not expired/revoked)
    /// </summary>
    /// <param name="refreshToken">Token to validate</param>
    /// <returns>True if token is valid</returns>
    public async Task<bool> IsValidAsync(string refreshToken)
    {
        var token = await GetByTokenAsync(refreshToken);
        return token is { IsRevoked: false } && token.ExpiresAt > DateTime.UtcNow;
    }

    /// <summary>
    /// Retrieves the user ID associated with a refresh token if the token is valid
    /// </summary>
    /// <param name="refreshToken">Token to look up</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    public async Task<Guid?> GetUserIdByTokenAsync(string refreshToken)
    {
        var token = await GetByTokenAsync(refreshToken);
        return token?.UserId;
    }

    /// <summary>
    /// Invalidates a refresh token by marking it as revoked
    /// </summary>
    /// <param name="refreshToken">Token to invalidate</param>
    public async Task InvalidateAsync(string refreshToken)
    {
        var token = await GetByTokenAsync(refreshToken);
        if (token is null) return;

        token.IsRevoked = true;
        await UpdateAsync(token);
    }

    private async Task<RefreshToken?> GetByTokenAsync(string tokenValue)
    {
        var request = new QueryRequest
        {
            TableName = TableName,
            IndexName = "TokenIndex",
            KeyConditionExpression = "Token = :v_token",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_token", new AttributeValue { S = tokenValue } }
            },
            Limit = 1
        };

        try
        {
            var response = await DynamoDbClient.QueryAsync(request);
            var item = response.Items.FirstOrDefault();
            return item == null ? null : MapItemToEntity(item);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error querying refresh token: {Token}", tokenValue);
            throw;
        }
    }
}

