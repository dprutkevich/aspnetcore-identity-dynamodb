using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class LogoutEndpointTests
{
    [Test]
    public async Task ShouldReturnBadRequest_WhenTokenIsMissing()
    {
        var mock = new Mock<IAuthService>();
        var result = await Logout.Execute(string.Empty, mock.Object);

        result.Should().BeOfType<BadRequest<string>>();
    }

    [Test]
    public async Task ShouldReturnNoContent_WhenLogoutSuccessful()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.LogoutAsync("valid-token"))
            .ReturnsAsync(Result.Success());

        var result = await Logout.Execute("valid-token", mock.Object);

        result.Should().BeOfType<NoContent>();
    }

    [Test]
    public async Task ShouldReturnProblem_WhenLogoutFails()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.LogoutAsync("invalid"))
            .ReturnsAsync(Result.Failure(Error.Problem("logout", "failed")));

        var result = await Logout.Execute("invalid", mock.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}