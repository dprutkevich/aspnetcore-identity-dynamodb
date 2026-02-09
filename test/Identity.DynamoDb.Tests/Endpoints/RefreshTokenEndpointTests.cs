using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Identity.DynamoDb.Tests.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class RefreshTokenEndpointTests
{
    [Test]
    public async Task ShouldReturnOk_WhenTokenValid()
    {
        var request = new RefreshTokenRequest("valid-token");
        var mock = new Mock<IAuthService>();

        mock.Setup(x => x.RefreshAccessTokenAsync(request.RefreshToken))
            .ReturnsAsync(Result.Success("new-access-token"));

        var result = await RefreshToken.Execute(request, mock.Object);

        result.Should().NotBeNull();
        result.GetField("AccessToken").Should().Be("new-access-token");
    }

    [Test]
    public async Task ShouldReturnProblem_WhenTokenInvalid()
    {
        var request = new RefreshTokenRequest("invalid-token");
        var mock = new Mock<IAuthService>();

        mock.Setup(x => x.RefreshAccessTokenAsync(request.RefreshToken))
            .ReturnsAsync(Result.Failure<string>(Error.Problem("invalid", "fail")));

        var result = await RefreshToken.Execute(request, mock.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}