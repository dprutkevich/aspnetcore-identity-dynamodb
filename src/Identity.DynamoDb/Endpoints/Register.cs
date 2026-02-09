using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Request model for user registration
/// </summary>
/// <param name="Email">User email address</param>
/// <param name="Password">User password</param>
public sealed record RegisterRequest(string Email, string Password);

/// <summary>
/// User registration endpoint
/// </summary>
public sealed class Register : IEndpoint
{
    /// <summary>
    /// Maps the register endpoint to the route builder
    /// </summary>
    /// <param name="builder">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/identity/register", ExecuteAsync)
            .AllowAnonymous()
            .WithTags("Identity")
            .WithSummary("User registration")
            .WithDescription("Registers a new user and returns JWT tokens")
            .Produces<object>(200)
            .Produces<object>(400);
    }

    /// <summary>
    /// Registers a new user and returns JWT tokens
    /// </summary>
    /// <param name="request"></param>
    /// <param name="authService"></param>
    /// <returns></returns>
    public static async Task<IResult> ExecuteAsync(RegisterRequest request, IAuthService authService)
    {
        var result = await authService.RegisterAsync(request.Email, request.Password);
        return result.Match(
            token => Results.Ok(new { token.AccessToken, token.RefreshToken }),
            CustomResults.Problem);
    }
}

