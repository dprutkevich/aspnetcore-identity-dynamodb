using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class ConfirmEmailEndpointTests
{
    [Test]
    public async Task ShouldReturnOk_WhenEmailConfirmed()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.ConfirmEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        var result = await ConfirmEmail.Execute(Guid.NewGuid(), "token-123", mock.Object);

        result.Should().BeOfType<Ok<string>>();
    }

    [Test]
    public async Task ShouldReturnProblem_WhenTokenInvalid()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.ConfirmEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Failure(Error.Problem("token", "invalid")));

        var result = await ConfirmEmail.Execute(Guid.NewGuid(), "bad-token", mock.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}
