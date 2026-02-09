using System.Security.Claims;
using FluentAssertions;
using Identity.DynamoDb.Endpoints;
using Identity.DynamoDb.Tests.Extensions;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class MeEndpointTests
{
    [Test]
    public void ShouldReturnUserInfo_FromClaims()
    {
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        }));

        var result = Me.Execute(principal);

        result.Should().NotBeNull();

        result.GetField("userId").Should().Be(userId);
        result.GetField("email").Should().Be(email);
    }
}

