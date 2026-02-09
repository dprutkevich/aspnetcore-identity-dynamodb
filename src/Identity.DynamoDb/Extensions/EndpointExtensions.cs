using System.Reflection;
using Identity.DynamoDb.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.DynamoDb.Extensions;

/// <summary>
/// Extension methods for registering and mapping endpoints
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Registers all endpoint implementations from the specified assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assembly">The assembly to scan for endpoints</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    /// <summary>
    /// Maps all registered UMS Identity endpoints
    /// </summary>
    /// <param name="app">The web application</param>
    /// <param name="prefix">Optional route prefix (default: empty)</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication MapUmsIdentityEndpoints(this WebApplication app, string prefix = "")
    {
        var endpoints = app.Services.GetServices<IEndpoint>();

        RouteGroupBuilder? groupBuilder = null;
        if (!string.IsNullOrEmpty(prefix))
        {
            groupBuilder = app.MapGroup(prefix);
        }

        IEndpointRouteBuilder builder = (IEndpointRouteBuilder?)groupBuilder ?? app;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    /// <summary>
    /// Maps endpoints using a custom route group builder
    /// </summary>
    /// <param name="app">The web application</param>
    /// <param name="routeGroupBuilder">The route group builder</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    /// <summary>
    /// Adds permission-based authorization to an endpoint
    /// </summary>
    /// <param name="app">The route handler builder</param>
    /// <param name="permission">The required permission</param>
    /// <returns>The route handler builder for chaining</returns>
    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder app, string permission)
    {
        return app.RequireAuthorization(permission);
    }
}
