using System.Security.Claims;
using Identity.DynamoDb.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Endpoint for retrieving current user information
/// </summary>
public sealed class Me : IEndpoint
{
    /// <summary>
    /// Maps the current user information endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/me", Execute)
            .RequireAuthorization()
            .WithTags("Identity");
    }

    /// <summary>
    /// Gets the current authenticated user's information
    /// </summary>
    /// <param name="user">The current user claims principal</param>
    /// <returns>User information from claims</returns>
    public static IResult Execute(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = user.FindFirstValue(ClaimTypes.Email)?.ToLower();

        return Results.Ok(new { userId, email });
    }
}

