using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Identity.DynamoDb.Abstractions;
using Serilog;

namespace Identity.DynamoDb.Repositories;

/// <summary>
/// Generic repository implementation for DynamoDB operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <param name="dynamoDbClient">DynamoDB client instance</param>
/// <param name="tableName">DynamoDB table name</param>
public class Repository<TEntity>(IAmazonDynamoDB dynamoDbClient, string tableName) : IRepository<TEntity> where TEntity : class, new()
{
    /// <summary>
    /// DynamoDB client instance
    /// </summary>
    protected readonly IAmazonDynamoDB DynamoDbClient = dynamoDbClient;
    
    /// <summary>
    /// DynamoDB table name
    /// </summary>
    protected readonly string TableName = tableName;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypePropertiesCache = new();

    private static readonly Dictionary<Type, Func<object, AttributeValue>> TypeToAttributeValueMap = new()
    {
        {typeof(string), value => new AttributeValue {S = (string)value}},
        {typeof(int), value => new AttributeValue {N = value.ToString()}},
        {typeof(int?), value => new AttributeValue {N = value.ToString()}},
        {typeof(bool), value => new AttributeValue {BOOL = (bool)value}},
        {typeof(bool?), value => new AttributeValue {BOOL = (bool)value}},
        {typeof(DateTime), value => new AttributeValue {S = ((DateTime)value).ToString("o")}},
        {typeof(DateTime?), value => new AttributeValue {S = ((DateTime)value).ToString("o")}},
        {typeof(Guid), value => new AttributeValue {S = value.ToString()}},
        {typeof(Guid?), value => new AttributeValue {S = value.ToString()}}
    };

    private static readonly Dictionary<Type, Func<AttributeValue, object?>> AttributeValueToTypeMap = new()
    {
        {typeof(string), av => av.S},
        {typeof(int), av => int.Parse(av.N)},
        {typeof(int?), av => av.N != null ? int.Parse(av.N) : (int?)null},
        {typeof(bool), av => av.BOOL},
        {typeof(bool?), av => av.NULL ? (bool?)null : av.BOOL},
        {typeof(DateTime), av => DateTime.Parse(av.S)},
        {typeof(DateTime?), av => av.S != null ? DateTime.Parse(av.S) : (DateTime?)null},
        {typeof(Guid), av => Guid.Parse(av.S)},
        {typeof(Guid?), av => av.S != null ? Guid.Parse(av.S) : (Guid?)null}
    };

    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>Entity if found, null otherwise</returns>
    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        try
        {
            var key = new Dictionary<string, AttributeValue>
            {
                {"Id", new AttributeValue {S = id.ToString()}}
            };

            var request = new GetItemRequest
            {
                TableName = TableName,
                Key = key,
                ConsistentRead = true
            };

            var response = await DynamoDbClient.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
                return null;

            return MapItemToEntity(response.Item);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GetByIdAsync for Id {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets all entities with optional limit
    /// </summary>
    /// <param name="limit">Maximum number of entities to return</param>
    /// <returns>Collection of entities</returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync(int limit = 100)
    {
        var items = new List<TEntity>();
        var request = new ScanRequest
        {
            TableName = TableName,
            Limit = limit
        };

        try
        {
            do
            {
                var response = await DynamoDbClient.ScanAsync(request);

                items.AddRange(response.Items.Select(MapItemToEntity));

                request.ExclusiveStartKey = response.LastEvaluatedKey;
            } while (request.ExclusiveStartKey != null && request.ExclusiveStartKey.Count > 0);

            return items;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GetAllAsync");
            throw;
        }
    }

    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">Entity to add</param>
    public async Task AddAsync(TEntity entity)
    {
        var item = MapEntityToItem(entity);

        var request = new PutItemRequest
        {
            TableName = TableName,
            Item = item,
            ConditionExpression = "attribute_not_exists(Id)"
        };

        try
        {
            await DynamoDbClient.PutItemAsync(request);
            Log.Information("Created {EntityType}.", typeof(TEntity).Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating {EntityType}.", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">Entity to update</param>
    public virtual async Task UpdateAsync(TEntity entity)
    {
        var id = GetEntityId(entity);

        var key = new Dictionary<string, AttributeValue>
    {
        { "Id", new AttributeValue { S = id } }
    };

        var attributeValues = MapEntityToItem(entity);

        var updateExpressions = new List<string>();
        var expressionAttributeValues = new Dictionary<string, AttributeValue>();
        var expressionAttributeNames = new Dictionary<string, string>();

        foreach (var kvp in attributeValues)
        {
            if (kvp.Key == "Id") continue;

            var attributeName = kvp.Key;
            var attributeAlias = $"#{attributeName}";
            var attributeValueKey = $":val_{attributeName}";

            updateExpressions.Add($"{attributeAlias} = {attributeValueKey}");
            expressionAttributeValues[attributeValueKey] = kvp.Value;
            expressionAttributeNames[attributeAlias] = attributeName;
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
            Log.Information("Updated {EntityType} with Id {Id}", typeof(TEntity).Name, id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating {EntityType} with Id {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    public async Task DeleteAsync(Guid id)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            {"Id", new AttributeValue {S = id.ToString()}}
        };

        var request = new DeleteItemRequest
        {
            TableName = TableName,
            Key = key
        };

        try
        {
            await DynamoDbClient.DeleteItemAsync(request);
            Log.Information("Deleted {EntityType} with Id {Id}", typeof(TEntity).Name, id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting {EntityType} with Id {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Maps a DynamoDB item to an entity
    /// </summary>
    /// <param name="item">DynamoDB item</param>
    /// <returns>Mapped entity</returns>
    protected TEntity MapItemToEntity(Dictionary<string, AttributeValue> item)
    {
        var entity = new TEntity();
        var properties = GetProperties(typeof(TEntity));

        foreach (var property in properties)
        {
            if (item.TryGetValue(property.Name, out var attributeValue))
            {
                if (AttributeValueToTypeMap.TryGetValue(property.PropertyType, out var converter))
                {
                    var value = converter(attributeValue);
                    property.SetValue(entity, value);
                }
                else if (attributeValue.S != null)
                {
                    try
                    {
                        var value = JsonSerializer.Deserialize(attributeValue.S, property.PropertyType);
                        property.SetValue(entity, value);
                    }
                    catch (JsonException ex)
                    {
                        Log.Warning(ex, "Failed to deserialize property {Property} with value: {Value}", property.Name, attributeValue.S);

                        if (property.PropertyType == typeof(Dictionary<string, object>))
                        {
                            property.SetValue(entity, new Dictionary<string, object>
                            {
                                ["migrated"] = true,
                                ["raw"] = attributeValue.S
                            });
                        }
                        else
                        {
                            property.SetValue(entity, GetDefault(property.PropertyType));
                        }
                    }
                }
            }
        }

        return entity;
    }

    private static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Maps an entity to a DynamoDB item
    /// </summary>
    /// <param name="entity">Entity to map</param>
    /// <returns>DynamoDB item</returns>
    protected Dictionary<string, AttributeValue> MapEntityToItem(TEntity entity)
    {
        var item = new Dictionary<string, AttributeValue>();
        var properties = GetProperties(typeof(TEntity));

        foreach (var property in properties)
        {
            var value = property.GetValue(entity);

            if (value != null)
            {
                if (TypeToAttributeValueMap.TryGetValue(property.PropertyType, out var converter))
                {
                    var attributeValue = converter(value);
                    if (attributeValue != null)
                    {
                        item[property.Name] = attributeValue;
                    }
                }
                else
                {
                    var json = JsonSerializer.Serialize(value);
                    item[property.Name] = new AttributeValue { S = json };
                }
            }
            else
            {
                item[property.Name] = new AttributeValue { NULL = true };
            }
        }

        return item;
    }

    /// <summary>
    /// Gets cached properties for a type
    /// </summary>
    /// <param name="type">Type to get properties for</param>
    /// <returns>Array of property info</returns>
    protected static PropertyInfo[] GetProperties(Type type)
    {
        return TypePropertiesCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }

    /// <summary>
    /// Gets the ID value from an entity as a string
    /// </summary>
    /// <param name="entity">Entity to get ID from</param>
    /// <returns>String representation of entity ID</returns>
    protected string GetEntityId(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty("Id");
        var idValue = idProperty?.GetValue(entity);

        if (idValue == null)
            throw new Exception("Id cannot be null.");

        return idValue.ToString() ?? string.Empty;
    }
}
