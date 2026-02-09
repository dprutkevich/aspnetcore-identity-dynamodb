using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Identity.DynamoDb.Configuration;
using Identity.DynamoDb.Models;
using Identity.DynamoDb.Repositories;
using Microsoft.Extensions.Options;
using Moq;

namespace Identity.DynamoDb.Tests;

[TestFixture]
public class IdentityUserRepositoryTests
{
    private Mock<IAmazonDynamoDB> _dynamoDbMock = null!;
    private IdentityUserRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _dynamoDbMock = new Mock<IAmazonDynamoDB>();
        var options = Options.Create(new IdentityOptions
        {
            Jwt = new JwtOptions { Secret = "x" },
            DynamoDb = new DynamoDbTablesOptions
            {
                UsersTable = "users",
                TokensTable = "t",
                TemporaryTokensTable = "temp_tokens",
                UserRolesTable = "user_roles"
            }
        });

        _repository = new IdentityUserRepository(_dynamoDbMock.Object, options);
    }

    [Test]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenFound()
    {
        var email = "test@example.com";
        var response = new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new() { { "Id", new AttributeValue { S = Guid.NewGuid().ToString() } }, { "Email", new AttributeValue { S = email } } }
            }
        };

        _dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default)).ReturnsAsync(response);

        var result = await _repository.GetByEmailAsync(email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Test]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNoMatch()
    {
        var response = new QueryResponse { Items = new List<Dictionary<string, AttributeValue>>() };
        _dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default)).ReturnsAsync(response);

        var result = await _repository.GetByEmailAsync("x@x.com");

        result.Should().BeNull();
    }

    [Test]
    public async Task GetByUserNameAsync_ShouldReturnUser_WhenFound()
    {
        var response = new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new() { { "Id", new AttributeValue { S = Guid.NewGuid().ToString() } }, { "NormalizedUserName", new AttributeValue { S = "USERNAME" } } }
            }
        };

        _dynamoDbMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), default)).ReturnsAsync(response);

        var result = await _repository.GetByUserNameAsync("username");

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateAsync_ShouldCallUpdateItem()
    {
        var user = new IdentityUser { Id = Guid.NewGuid(), Email = "u@x.com" };
        _dynamoDbMock.Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default))
            .ReturnsAsync(new UpdateItemResponse());

        await _repository.UpdateAsync(user);

        _dynamoDbMock.Verify(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default), Times.Once);
    }
}