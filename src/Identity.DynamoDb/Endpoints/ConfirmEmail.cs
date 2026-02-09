using Identity.DynamoDb.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Identity.DynamoDb.Common;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Endpoint for confirming user email address
/// </summary>
public sealed class ConfirmEmail : IEndpoint
{
    /// <summary>
    /// Maps the confirm email endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/identity/confirm-email", Execute)
            .AllowAnonymous()
            .WithTags("Identity");

    /// <summary>
    /// Confirms the user's email address using the provided token
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="token">The email confirmation token</param>
    /// <param name="authService">The authentication service</param>
    /// <returns>Result indicating success or failure</returns>
    public static async Task<IResult> Execute(Guid userId, string token, IAuthService authService)
    {
        var result = await authService.ConfirmEmailAsync(userId, token);
        return result.Match(
            () => Results.Ok("? Email confirmed successfully"),
            CustomResults.Problem);
    }
}
