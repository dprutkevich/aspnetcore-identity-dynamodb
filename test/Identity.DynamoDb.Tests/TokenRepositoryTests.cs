using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Identity.DynamoDb.Configuration;
using Identity.DynamoDb.Repositories;
using Microsoft.Extensions.Options;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class TokenRepositoryTests
{
    private Mock<IAmazonDynamoDB> _dynamoDbMock = null!;
    private TokenRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _dynamoDbMock = new Mock<IAmazonDynamoDB>();
        var options = Options.Create(new IdentityOptions
        {
            Jwt = new JwtOptions { Secret = "x" },
            DynamoDb = new DynamoDbTablesOptions
            {
                UsersTable = "u",
                TokensTable = "tokens",
                TemporaryTokensTable = "temp_tokens",
                UserRolesTable = "user_roles"
            }
        });

        _repository = new TokenRepository(_dynamoDbMock.Object, options);
    }

    [Test]
    public async Task StoreRefreshTokenAsync_ShouldAddToken()
    {
        var userId = Guid.NewGuid();
        var token = Guid.NewGuid().ToString();

        _dynamoDbMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default))
            .ReturnsAsync(new PutItemResponse());

        await _repository.StoreRefreshTokenAsync(userId, token, DateTime.UtcNow.AddMinutes(30));

        _dynamoDbMock.Verify(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default), Times.Once);
    }

    [Test]
    public async Task GetUserIdByTokenAsync_ShouldReturnUserId_WhenTokenExists()
    {
        var userId = Guid.NewGuid();
        var token = "abc123";

        _dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default)).ReturnsAsync(
            new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>>
                {
                    new()
                    {
                        { "UserId", new AttributeValue { S = userId.ToString() } },
                        { "Token", new AttributeValue { S = token } },
                        { "ExpiresAt", new AttributeValue { S = DateTime.UtcNow.AddMinutes(5).ToString("o") } },
                        { "IsRevoked", new AttributeValue { BOOL = false } }
                    }
                }
            });

        var result = await _repository.GetUserIdByTokenAsync(token);

        result.Should().Be(userId);
    }

    [Test]
    public async Task IsValidAsync_ShouldReturnFalse_WhenTokenRevoked()
    {
        _dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default)).ReturnsAsync(
            new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>>
                {
                    new()
                    {
                        { "Token", new AttributeValue { S = "xyz" } },
                        { "ExpiresAt", new AttributeValue { S = DateTime.UtcNow.AddMinutes(10).ToString("o") } },
                        { "IsRevoked", new AttributeValue { BOOL = true } }
                    }
                }
            });

        var result = await _repository.IsValidAsync("xyz");

        result.Should().BeFalse();
    }

    [Test]
    public async Task InvalidateAsync_ShouldCallUpdateItem_WhenTokenFound()
    {
        var token = "abc";

        _dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default)).ReturnsAsync(
            new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>>
                {
                    new()
                    {
                        { "Id", new AttributeValue { S = Guid.NewGuid().ToString() } },
                        { "UserId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                        { "Token", new AttributeValue { S = token } },
                        { "ExpiresAt", new AttributeValue { S = DateTime.UtcNow.AddMinutes(10).ToString("o") } },
                        { "IsRevoked", new AttributeValue { BOOL = false } }
                    }
                }
            });

        _dynamoDbMock.Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default))
            .ReturnsAsync(new UpdateItemResponse());

        await _repository.InvalidateAsync(token);

        _dynamoDbMock.Verify(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default), Times.Once);
    }
}
