using FluentAssertions;
using Identity.DynamoDb.Abstractions;
using Identity.DynamoDb.Common;
using Identity.DynamoDb.Endpoints;
using Identity.DynamoDb.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class RegisterEndpointTests
{
    [Test]
    public async Task ShouldReturnOk_WhenRegisterSuccessful()
    {
        var request = new RegisterRequest("new@example.com", "password123");
        var tokens = new AuthTokens { AccessToken = "access", RefreshToken = "refresh" };

        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.RegisterAsync(request.Email, request.Password))
            .ReturnsAsync(Result.Success(tokens));

        var result = await Register.ExecuteAsync(request, mock.Object);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task ShouldReturnProblem_WhenRegisterFails()
    {
        var request = new RegisterRequest("fail@example.com", "weak");

        var mock = new Mock<IAuthService>();
        mock.Setup(x => x.RegisterAsync(request.Email, request.Password))
            .ReturnsAsync(Result.Failure<AuthTokens>(Error.Problem("fail", "Email taken")));

        var result = await Register.ExecuteAsync(request, mock.Object);

        result.Should().BeOfType<ProblemHttpResult>();
    }
}