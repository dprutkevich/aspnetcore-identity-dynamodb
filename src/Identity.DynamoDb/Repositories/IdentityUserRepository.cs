using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Models;
using Identity.DynamoDb.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace Identity.DynamoDb.Repositories;

/// <summary>
/// Repository for managing users in DynamoDB
/// </summary>
/// <param name="dynamoDbClient">DynamoDB client instance</param>
/// <param name="options">Identity configuration options</param>
public class IdentityUserRepository(
    IAmazonDynamoDB dynamoDbClient,
    IOptions<IdentityOptions> options
)
    : Repository<IdentityUser>(dynamoDbClient, options.Value.DynamoDb.UsersTable), IIdentityUserRepository
{
    /// <summary>
    /// Retrieves a user by their username (case-insensitive)
    /// </summary>
    /// <param name="userName">Username to search for</param>
    /// <returns>User if found, null otherwise</returns>
    public async Task<IdentityUser?> GetByUserNameAsync(string userName)
    {
        // UserName is treated as email for compatibility with current schema.
        return await GetByEmailAsync(userName);
    }

    /// <summary>
    /// Retrieves a user by their email address (case-insensitive)
    /// </summary>
    /// <param name="email">Email address to search for</param>
    /// <returns>User if found, null otherwise</returns>
    public async Task<IdentityUser?> GetByEmailAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        var request = new QueryRequest
        {
            TableName = TableName,
            IndexName = "EmailIndex",
            KeyConditionExpression = "Email = :v_email",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_email", new AttributeValue { S = normalizedEmail } }
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
            Log.Error(ex, "Error retrieving user by email: {Email}", normalizedEmail);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user in the database
    /// </summary>
    /// <param name="user">User to update</param>
    public override async Task UpdateAsync(IdentityUser user)
    {
        user.Email = NormalizeEmail(user.Email);

        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = user.Id.ToString() } }
        };

        var attributeValues = MapEntityToItem(user);
        var updateExpressions = new List<string>();
        var expressionAttributeValues = new Dictionary<string, AttributeValue>();
        var expressionAttributeNames = new Dictionary<string, string>();

        foreach (var kvp in attributeValues)
        {
            if (kvp.Key is "UserId" or "Id") continue;

            var attributeName = kvp.Key;
            var attributeValueKey = $":val_{attributeName}";
            var attributeNameAlias = $"#{attributeName}";

            updateExpressions.Add($"{attributeNameAlias} = {attributeValueKey}");
            expressionAttributeValues[attributeValueKey] = kvp.Value;
            expressionAttributeNames[attributeNameAlias] = attributeName;
        }

        var updateExpression = "SET " + string.Join(", ", updateExpressions);

        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = key,
            UpdateExpression = updateExpression,
            ExpressionAttributeValues = expressionAttributeValues,
            ExpressionAttributeNames = expressionAttributeNames
        };

        try
        {
            await DynamoDbClient.UpdateItemAsync(request);
            Log.Information("Updated User with Id {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating User with Id {UserId}", user.Id);
            throw;
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
