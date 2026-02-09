using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests.Endpoints;

[TestFixture]
public class ForgotPasswordEndpointTests
{
    [Test]
    public async Task ShouldReturnNoContent_WhenRequestIsSuccessful()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.GeneratePasswordResetTokenAsync("test@example.com"))
            .ReturnsAsync(Result.Success());

        var request = new ForgotPasswordRequest("test@example.com");

        var result = await ForgotPassword.Execute(request, mock.Object);

        result.Should().BeOfType<NoContent>();
    }

    [Test]
    public async Task ShouldReturnProblem_WhenServiceFails()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.GeneratePasswordResetTokenAsync("fail@example.com"))
            .ReturnsAsync(Result.Failure(Error.Problem("fail", "something went wrong")));

        var request = new ForgotPasswordRequest("fail@example.com");

        var result = await ForgotPassword.Execute(request, mock.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}