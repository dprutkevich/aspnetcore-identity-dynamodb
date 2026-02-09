using Identity.DynamoDb.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using Identity.DynamoDb.Common;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Request model for changing user password
/// </summary>
/// <param name="OldPassword">Current password</param>
/// <param name="NewPassword">New password</param>
public sealed record ChangePasswordRequest(string OldPassword, string NewPassword);

/// <summary>
/// Endpoint for changing user password
/// </summary>
public sealed class ChangePassword : IEndpoint
{
    /// <summary>
    /// Maps the change password endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app) => 
        app.MapPost("/api/identity/change-password", ChangeUserPassword)
            .RequireAuthorization()
            .WithTags("Identity");

    /// <summary>
    /// Changes the password for the authenticated user
    /// </summary>
    /// <param name="request">The change password request</param>
    /// <param name="user">The current user claims principal</param>
    /// <param name="authService">The authentication service</param>
    /// <returns>Result indicating success or failure</returns>
    public static async Task<IResult> ChangeUserPassword(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        IAuthService authService)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Results.Unauthorized();

        var result = await authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
        
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
