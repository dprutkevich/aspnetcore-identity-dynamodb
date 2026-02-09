using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Request model for forgot password functionality
/// </summary>
/// <param name="Email">Email address for password reset</param>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>
/// Endpoint for initiating password reset process
/// </summary>
public sealed class ForgotPassword : IEndpoint
{
    /// <summary>
    /// Maps the forgot password endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/forgot-password", Execute)
            .AllowAnonymous()
            .WithTags("Identity");
    }

    /// <summary>
    /// Initiates the password reset process for the specified email
    /// </summary>
    /// <param name="request">The forgot password request</param>
    /// <param name="authService">The authentication service</param>
    /// <returns>Result indicating the operation was initiated</returns>
    public static async Task<IResult> Execute(ForgotPasswordRequest request, IAuthService authService)
    {
        var result = await authService.GeneratePasswordResetTokenAsync(request.Email);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}

