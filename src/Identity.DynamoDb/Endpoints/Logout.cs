using Identity.DynamoDb.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Identity.DynamoDb.Common;
using Microsoft.AspNetCore.Mvc;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Endpoint for user logout functionality
/// </summary>
public sealed class Logout : IEndpoint
{
    /// <summary>
    /// Maps the logout endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/logout", Execute)
            .RequireAuthorization()
            .WithTags("Identity");
    }

    /// <summary>
    /// Logs out the user by invalidating their refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token to invalidate</param>
    /// <param name="authService">The authentication service</param>
    /// <returns>Result indicating success or failure</returns>
    public static async Task<IResult> Execute(
        [FromHeader(Name = "refreshtoken")] string refreshToken,
        IAuthService authService)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Results.BadRequest("Refresh token is required.");

        var result = await authService.LogoutAsync(refreshToken);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
