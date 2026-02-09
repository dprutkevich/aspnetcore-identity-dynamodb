namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Generic repository interface for CRUD operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Retrieves an entity by its unique identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<TEntity?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves all entities with optional limit
    /// </summary>
    /// <param name="limit">Maximum number of entities to return</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(int limit);
    
    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">Entity to add</param>
    Task AddAsync(TEntity entity);
    
    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">Entity to update</param>
    Task UpdateAsync(TEntity entity);
    
    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    Task DeleteAsync(Guid id);
}
