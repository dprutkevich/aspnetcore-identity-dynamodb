using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class ResetPasswordEndpointTests
{
    [Test]
    public async Task ShouldReturnNoContent_WhenResetSuccessful()
    {
        var request = new ResetPasswordRequest("test@example.com", "token", "newpass");
        var mock = new Mock<IAuthService>();

        mock.Setup(x => x.ResetPasswordAsync(request.Email, request.Token, request.NewPassword))
            .ReturnsAsync(Result.Success());

        var result = await ResetPassword.Execute(request, mock.Object);

        result.Should().BeOfType<NoContent>();
    }

    [Test]
    public async Task ShouldReturnProblem_WhenResetFails()
    {
        var request = new ResetPasswordRequest("test@example.com", "bad-token", "newpass");
        var mock = new Mock<IAuthService>();

        mock.Setup(x => x.ResetPasswordAsync(request.Email, request.Token, request.NewPassword))
            .ReturnsAsync(Result.Failure(Error.Problem("fail", "invalid")));

        var result = await ResetPassword.Execute(request, mock.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}