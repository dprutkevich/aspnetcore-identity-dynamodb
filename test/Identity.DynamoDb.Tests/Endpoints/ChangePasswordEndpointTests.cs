using System.Security.Claims;
using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests.Endpoints;

[TestFixture]
public class ChangePasswordEndpointTests
{
    [Test]
    public async Task ShouldReturnUnauthorized_IfUserIdIsMissing()
    {
        var mockService = new Mock<IAuthService>();
        var request = new ChangePasswordRequest("old", "new");
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // no claims

        var result = await ChangePassword.ChangeUserPassword(request, principal, mockService.Object);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Test]
    public async Task ShouldReturnNoContent_IfPasswordChanged()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("old", "new");
        var mockService = new Mock<IAuthService>();

        mockService.Setup(x => x.ChangePasswordAsync(userId, "old", "new"))
            .ReturnsAsync(Result.Success());

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

        var result = await ChangePassword.ChangeUserPassword(request, claims, mockService.Object);

        result.Should().BeOfType<NoContent>();
    }

    [Test]
    public async Task ShouldReturnProblem_IfServiceFails()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("old", "new");
        var mockService = new Mock<IAuthService>();

        mockService.Setup(x => x.ChangePasswordAsync(userId, "old", "new"))
            .ReturnsAsync(Result.Failure(Error.Problem("fail", "bad")));

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

        var result = await ChangePassword.ChangeUserPassword(request, claims, mockService.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}