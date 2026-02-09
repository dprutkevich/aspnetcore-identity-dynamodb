using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Routing;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Request model for resetting user password
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Token">Password reset token</param>
/// <param name="NewPassword">New password to set</param>
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);

/// <summary>
/// Endpoint for resetting user password with token
/// </summary>
public sealed class ResetPassword: IEndpoint
{ 
    /// <summary>
    /// Maps the reset password endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/reset-password", Execute)
            .AllowAnonymous()
            .WithTags("Identity");
    }

    /// <summary>
    /// Resets the user's password using the provided token
    /// </summary>
    /// <param name="request">The password reset request</param>
    /// <param name="authService">The authentication service</param>
    /// <returns>Result indicating success or failure</returns>
    public static async Task<IResult> Execute(ResetPasswordRequest request, IAuthService authService)
    {
        var result = await authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}

