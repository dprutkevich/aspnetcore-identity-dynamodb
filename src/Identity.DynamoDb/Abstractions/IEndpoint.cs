using Microsoft.AspNetCore.Routing;

namespace Identity.DynamoDb.Abstractions;

/// <summary>
/// Interface for defining API endpoints
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the route builder
    /// </summary>
    /// <param name="builder">Endpoint route builder</param>
    void MapEndpoint(IEndpointRouteBuilder builder);
}
