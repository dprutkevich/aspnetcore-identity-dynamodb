using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.DynamoDb.Endpoints;

/// <summary>
/// Request model for sending email confirmation
/// </summary>
/// <param name="UserId">User ID to send confirmation email to</param>
public sealed record SendConfirmationEmailRequest(Guid UserId);

/// <summary>
/// Endpoint for sending email confirmation
/// </summary>
public sealed class SendConfirmationEmail : IEndpoint
{
    /// <summary>
    /// Maps the send confirmation email endpoint to the route builder
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/send-confirmation-email", Execute)
        .AllowAnonymous()
        .WithTags("Identity");
    }

    /// <summary>
    /// Sends an email confirmation to the specified user
    /// </summary>
    /// <param name="request">The send confirmation email request</param>
    /// <param name="authService">The authentication service</param>
    /// <returns>Result indicating the email was sent</returns>
    public static async Task<IResult> Execute(SendConfirmationEmailRequest request, IAuthService authService)
    {
        var result = await authService.SendConfirmationEmailAsync(request.UserId);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}

