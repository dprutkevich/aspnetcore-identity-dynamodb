using Identity.DynamoDb.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Identity.DynamoDb.Common;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Request model for refreshing access token
/// </summary>
/// <param name="RefreshToken">The refresh token to use for generating new access token</param>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Endpoint for refreshing access tokens
/// </summary>
public class RefreshToken : IEndpoint
{
    /// <summary>
    /// Maps the refresh token endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/refresh-token", Execute)
            .AllowAnonymous()
            .WithTags("Identity");
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <param name="authService">The authentication service</param>
    /// <returns>New access token or error</returns>
    public static async Task<IResult> Execute(RefreshTokenRequest request, IAuthService authService)
    {
        var result = await authService.RefreshAccessTokenAsync(request.RefreshToken);

        return result.Match(
            token => Results.Ok(new { AccessToken = token }),
            CustomResults.Problem);
    }
}
