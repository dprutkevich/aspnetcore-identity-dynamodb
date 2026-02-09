using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Configuration;
using Identity.DynamoDb.Models;
using Microsoft.Extensions.Options;

namespace Identity.DynamoDb.Repositories;

/// <summary>
/// DynamoDB implementation of user role repository
/// </summary>
public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    /// <summary>
    /// Initializes a new instance of the UserRoleRepository
    /// </summary>
    /// <param name="dynamoDb">DynamoDB client</param>
    /// <param name="options">Identity options</param>
    public UserRoleRepository(IAmazonDynamoDB dynamoDb, IOptions<IdentityOptions> options) 
        : base(dynamoDb, options.Value.DynamoDb.UserRolesTable)
    {
    }

    /// <summary>
    /// Retrieves all roles for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of user roles</returns>
    public async Task<List<UserRole>> GetByUserIdAsync(Guid userId)
    {
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            FilterExpression = "UserId = :userId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":userId"] = new(userId.ToString())
            }
        };

        var response = await DynamoDbClient.ScanAsync(scanRequest);
        return response.Items.Select(MapFromDynamoDb).ToList();
    }

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if user has the role</returns>
    public async Task<bool> HasRoleAsync(Guid userId, string roleName)
    {
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            FilterExpression = "UserId = :userId AND RoleName = :roleName",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":userId"] = new(userId.ToString()),
                [":roleName"] = new(roleName)
            }
        };

        var response = await DynamoDbClient.ScanAsync(scanRequest);
        return response.Items.Any();
    }

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="roleName">Role name to assign</param>
    /// <param name="assignedBy">Who assigned the role (optional)</param>
    public async Task AddRoleAsync(Guid userId, string roleName, Guid? assignedBy = null)
    {
        // Check if role already exists
        if (await HasRoleAsync(userId, roleName))
            return;

        var userRole = UserRole.Create(userId, roleName, assignedBy);
        await AddAsync(userRole);
    }

    /// <summary>
    /// Removes a role from a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="roleName">Role name to remove</param>
    public async Task RemoveRoleAsync(Guid userId, string roleName)
    {
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            FilterExpression = "UserId = :userId AND RoleName = :roleName",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":userId"] = new(userId.ToString()),
                [":roleName"] = new(roleName)
            }
        };

        var response = await DynamoDbClient.ScanAsync(scanRequest);
        var item = response.Items.FirstOrDefault();
        
        if (item != null)
        {
            var roleId = Guid.Parse(item["Id"].S);
            await DeleteAsync(roleId);
        }
    }

    /// <summary>
    /// Gets all role names for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of role names</returns>
    public async Task<List<string>> GetRoleNamesAsync(Guid userId)
    {
        var roles = await GetByUserIdAsync(userId);
        return roles.Select(r => r.RoleName).ToList();
    }

    private static UserRole MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new UserRole
        {
            Id = Guid.Parse(item["Id"].S),
            UserId = Guid.Parse(item["UserId"].S),
            RoleName = item["RoleName"].S,
            AssignedAt = DateTime.Parse(item["AssignedAt"].S),
            AssignedBy = item.ContainsKey("AssignedBy") && !string.IsNullOrEmpty(item["AssignedBy"].S) 
                ? Guid.Parse(item["AssignedBy"].S) 
                : null
        };
    }
}
