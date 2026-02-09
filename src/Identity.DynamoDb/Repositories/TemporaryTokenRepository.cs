using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Configuration;
using Identity.DynamoDb.Models;
using Microsoft.Extensions.Options;

namespace Identity.DynamoDb.Repositories;

/// <summary>
/// DynamoDB implementation of temporary token repository
/// </summary>
public class TemporaryTokenRepository : Repository<TemporaryToken>, ITemporaryTokenRepository
{
    /// <summary>
    /// Initializes a new instance of the TemporaryTokenRepository
    /// </summary>
    /// <param name="dynamoDb">DynamoDB client</param>
    /// <param name="options">Identity options</param>
    public TemporaryTokenRepository(IAmazonDynamoDB dynamoDb, IOptions<IdentityOptions> options) 
        : base(dynamoDb, options.Value.DynamoDb.TemporaryTokensTable)
    {
    }

    /// <summary>
    /// Retrieves a token by its value and type
    /// </summary>
    /// <param name="token">Token value</param>
    /// <param name="type">Token type</param>
    /// <returns>Token if found, null otherwise</returns>
    public async Task<TemporaryToken?> GetByTokenAsync(string token, TokenType type)
    {
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            FilterExpression = "#token = :token AND #type = :type",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#token"] = "Token",
                ["#type"] = "Type"
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":token"] = new(token),
                [":type"] = new(type.ToString())
            }
        };

        var response = await DynamoDbClient.ScanAsync(scanRequest);
        var item = response.Items.FirstOrDefault();
        
        return item != null ? MapFromDynamoDb(item) : null;
    }

    /// <summary>
    /// Retrieves all tokens for a user by type
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="type">Token type</param>
    /// <returns>List of tokens</returns>
    public async Task<List<TemporaryToken>> GetByUserIdAsync(Guid userId, TokenType type)
    {
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            FilterExpression = "UserId = :userId AND #type = :type",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#type"] = "Type"
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":userId"] = new(userId.ToString()),
                [":type"] = new(type.ToString())
            }
        };

        var response = await DynamoDbClient.ScanAsync(scanRequest);
        return response.Items.Select(MapFromDynamoDb).ToList();
    }

    /// <summary>
    /// Marks a token as used
    /// </summary>
    /// <param name="tokenId">Token identifier</param>
    public async Task MarkAsUsedAsync(Guid tokenId)
    {
        var updateRequest = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new(tokenId.ToString())
            },
            UpdateExpression = "SET IsUsed = :isUsed",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":isUsed"] = new() { BOOL = true }
            }
        };

        await DynamoDbClient.UpdateItemAsync(updateRequest);
    }

    /// <summary>
    /// Removes expired tokens from storage
    /// </summary>
    public async Task CleanupExpiredTokensAsync()
    {
        var now = DateTime.UtcNow;
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            FilterExpression = "ExpiresAt < :now",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":now"] = new(now.ToString("O"))
            }
        };

        var response = await DynamoDbClient.ScanAsync(scanRequest);
        
        foreach (var item in response.Items)
        {
            var deleteRequest = new DeleteItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["Id"] = item["Id"]
                }
            };
            
            await DynamoDbClient.DeleteItemAsync(deleteRequest);
        }
    }

    /// <summary>
    /// Invalidates all tokens of a specific type for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="type">Token type to invalidate</param>
    public async Task InvalidateUserTokensAsync(Guid userId, TokenType type)
    {
        var tokens = await GetByUserIdAsync(userId, type);
        
        foreach (var token in tokens.Where(t => t.IsValid))
        {
            await MarkAsUsedAsync(token.Id);
        }
    }

    private static TemporaryToken MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new TemporaryToken
        {
            Id = Guid.Parse(item["Id"].S),
            UserId = Guid.Parse(item["UserId"].S),
            Token = item["Token"].S,
            Type = Enum.Parse<TokenType>(item["Type"].S),
            ExpiresAt = DateTime.Parse(item["ExpiresAt"].S),
            CreatedAt = DateTime.Parse(item["CreatedAt"].S),
            IsUsed = item.ContainsKey("IsUsed") && item["IsUsed"].BOOL
        };
    }
}
