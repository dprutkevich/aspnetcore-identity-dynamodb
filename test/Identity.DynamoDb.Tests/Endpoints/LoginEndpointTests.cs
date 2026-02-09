using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Identity.DynamoDb.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class LoginEndpointTests
{
    [Test]
    public async Task ShouldReturnOk_WhenLoginSuccessful()
    {
        var request = new LoginRequest("test@example.com", "secret");
        var mock = new Mock<IAuthService>();
        var tokens = new AuthTokens { AccessToken = "access", RefreshToken = "refresh" };

        mock.Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(Result.Success(tokens));

        var result = await Login.ExecuteAsync(request, mock.Object);

        result.GetType().GetProperty("StatusCode")?.GetValue(result).Should().Be(200);
        
        var value = result.GetType().GetProperty("Value")?.GetValue(result);

        value.Should().BeEquivalentTo(new
        {
            AccessToken = "access",
            RefreshToken = "refresh"
        });
    }

    [Test]
    public async Task ShouldReturnProblem_WhenLoginFails()
    {
        var request = new LoginRequest("fail@example.com", "bad");
        var mock = new Mock<IAuthService>();

        mock.Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(Result.Failure<AuthTokens>(Error.Problem("fail", "wrong")));

        var result = await Login.ExecuteAsync(request, mock.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}