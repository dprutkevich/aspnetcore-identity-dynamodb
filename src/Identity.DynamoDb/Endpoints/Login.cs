using Identity.DynamoDb.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Identity.DynamoDb.Common;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Request model for user login
/// </summary>
/// <param name="Email">User email address</param>
/// <param name="Password">User password</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// User login endpoint
/// </summary>
public sealed class Login : IEndpoint
{
    /// <summary>
    /// Maps the login endpoint to the route builder
    /// </summary>
    /// <param name="builder">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/identity/login", ExecuteAsync)
            .AllowAnonymous()
            .WithTags("Identity")
            .WithSummary("User login")
            .WithDescription("Authenticates user and returns JWT tokens")
            .Produces<object>(200)
            .Produces<object>(400)
            .Produces<object>(401);
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens
    /// </summary>
    /// <param name="request"></param>
    /// <param name="authService"></param>
    /// <returns></returns>
    public static async Task<IResult> ExecuteAsync(LoginRequest request, IAuthService authService)
    {
        var result = await authService.LoginAsync(request.Email, request.Password);
        return result.Match(
            onSuccess: tokens => Results.Ok(new { tokens.AccessToken, tokens.RefreshToken }),
            onFailure: CustomResults.Problem);
    }
}
